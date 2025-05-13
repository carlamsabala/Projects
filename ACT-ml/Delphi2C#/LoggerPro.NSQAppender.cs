
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using LoggerPro; 
using System.Threading.Tasks;
using System.Web;

namespace LoggerPro.Appenders
{
    
    public delegate void OnCreateData(object sender, TLogItem logItem, out Stream data);
    public delegate void OnNetSendError(object sender, TLogItem logItem, Exception netError, ref int retryCount);

    
    public class LoggerProNSQAppenderBase : LoggerProAppenderBase, ILogAppender
    {
        public const string DEFAULT_NSQ_URL = "http://127.0.0.1:4151";

        
        private OnCreateData _onCreateData;
        private OnNetSendError _onNetSendError;

        
        private string _nsqUrl;
        private string _topic;
        private bool _ephemeral;
        
        private string _userName;
        private string _machineName;
        private string _lastSignature;

        public LoggerProNSQAppenderBase(string topic = "", bool ephemeral = false, string nsqUrl = DEFAULT_NSQ_URL, ILogItemRenderer logItemRenderer = null)
            : base(logItemRenderer)
        {
            
            _ephemeral = ephemeral;
            _nsqUrl = nsqUrl; 
            _userName = nsqUrl; 
            _topic = topic;
        }

       
        public string NSQUrl
        {
            get => _nsqUrl;
            set => _nsqUrl = value;
        }

        
        public string Topic
        {
            get => _topic;
            set => _topic = value;
        }

        
        public bool Ephemeral
        {
            get => _ephemeral;
            set => _ephemeral = value;
        }

        public OnCreateData OnCreateData
        {
            get => _onCreateData;
            set => _onCreateData = value;
        }

        public OnNetSendError OnNetSendError
        {
            get => _onNetSendError;
            set => _onNetSendError = value;
        }

        
        public override void Setup()
        {
            base.Setup();
            
        }

        
        public override void TearDown()
        {
            base.TearDown();
            
        }

        
        public virtual Stream CreateData(TLogItem srcLogItem)
        {
            try
            {
                if (OnCreateData != null)
                {
                    OnCreateData(this, srcLogItem, out Stream data);
                    return data;
                }
                else
                {
                    
                    string logText = FormatLog(srcLogItem);
                    return new MemoryStream(Encoding.UTF8.GetBytes(logText));
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        
        public override void WriteLog(TLogItem logItem)
        {
            int retryCount = 0;
            
            using (HttpClient httpClient = new HttpClient())
            {
                
                string topicName = string.IsNullOrWhiteSpace(Topic) ? logItem.LogTag.Trim() : Topic.Trim();

                
                string uri = NSQUrl.TrimEnd('/') + "/pub?topic=" + HttpUtility.UrlEncode(topicName);
                if (Ephemeral)
                {
                    uri += "#ephemeral";
                }

                
                Stream data = CreateData(logItem);
                if (data != null)
                {
                    while (true)
                    {
                        try
                        {
                            
                            httpClient.Timeout = TimeSpan.FromMilliseconds(200);
                            data.Seek(0, SeekOrigin.Begin);
                            
                            using (var content = new StreamContent(data))
                            {
                                
                                content.Headers.ContentType = new MediaTypeHeaderValue("text/plain")
                                {
                                    CharSet = "utf-8"
                                };
                                
                                HttpResponseMessage response = httpClient.PostAsync(uri, content).Result;
                                response.EnsureSuccessStatusCode();
                            }
                            break;
                        }
                        catch (HttpRequestException ex)
                        {
                            
                            if (OnNetSendError != null)
                            {
                                OnNetSendError(this, logItem, ex, ref retryCount);
                            }
                            retryCount++;
                            if (retryCount >= 5) 
                            {
                                
                                throw;
                            }
                            
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                }
            }
        }
    }
}
