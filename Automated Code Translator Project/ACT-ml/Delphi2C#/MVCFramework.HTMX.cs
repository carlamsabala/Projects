
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace MVCFramework.HTMX
{
    #region Enums

    public enum TClientEventType
    {
        Received,
        Settled,
        Swapped
    }

    public enum TSwapOption
    {
        InnerHTML,
        OuterHTML,
        BeforeBegin,
        AfterBegin,
        BeforeEnd,
        AfterEnd,
        Delete,
        None
    }

    public enum TShowScrollType
    {
        None,
        Show,
        Scroll
    }

    public enum TSwapScrollTo
    {
        Top,
        Bottom
    }

    #endregion

    #region Header Constant Helpers

    public static class HTMXRequestHeaders
    {
        public const string CurrentUrl = "HX-Current-URL";
        public const string HistoryRestoreRequest = "HX-History-Restore-Request";
        public const string Prompt = "HX-Prompt";
        public const string Request = "HX-Request";
        public const string Target = "HX-Target";
        public const string TriggerName = "HX-Trigger-Name";
        public const string Trigger = "HX-Trigger";
        public const string Boosted = "HX-Boosted";
        public const string TriggeringEvent = "Triggering-Event";
    }

    public static class HTMXResponseHeaders
    {
        public const string Location = "HX-Location";
        public const string Refresh = "HX-Refresh";
        public const string PushURL = "HX-Push-Url";
        public const string Redirect = "HX-Redirect";
        public const string ReplaceURL = "HX-Replace-Url";
        public const string Reselect = "HX-Reselect";
        public const string Reswap = "HX-Reswap";
        public const string Retarget = "HX-Retarget";
        public const string Trigger = "HX-Trigger";
        public const string TriggerAfterSettle = "HX-Trigger-After-Settle";
        public const string TriggerAfterSwap = "HX-Trigger-After-Swap";
    }

    #endregion

    #region WebRequest Extensions

    public static class HTMXRequestExtensions
    {
        public static string GetHtmxHeader(this TMVCWebRequest req, string header)
        {
            return req.Headers.ContainsKey(header) ? req.Headers[header] : string.Empty;
        }

        public static bool GetHtmxHeaderToBool(this TMVCWebRequest req, string header)
        {
            return string.Equals(req.GetHtmxHeader(header), "true", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasHeader(this TMVCWebRequest req, string header)
        {
            return !string.IsNullOrEmpty(req.GetHtmxHeader(header));
        }

        public static bool IsHTMX(this TMVCWebRequest req)
        {
            return req.GetHtmxHeaderToBool(HTMXRequestHeaders.Request);
        }

        public static bool HXIsBoosted(this TMVCWebRequest req)
        {
            return req.GetHtmxHeaderToBool(HTMXRequestHeaders.Boosted);
        }

        public static bool HXIsHistoryRestoreRequest(this TMVCWebRequest req)
        {
            return req.GetHtmxHeaderToBool(HTMXRequestHeaders.HistoryRestoreRequest);
        }

        public static string HXGetCurrentUrl(this TMVCWebRequest req)
        {
            return req.GetHtmxHeader(HTMXRequestHeaders.CurrentUrl);
        }

        public static string HXGetPrompt(this TMVCWebRequest req)
        {
            return req.GetHtmxHeader(HTMXRequestHeaders.Prompt);
        }

        public static string HXGetTarget(this TMVCWebRequest req)
        {
            return req.GetHtmxHeader(HTMXRequestHeaders.Target);
        }

        public static string HXGetTrigger(this TMVCWebRequest req)
        {
            return req.GetHtmxHeader(HTMXRequestHeaders.Trigger);
        }

        public static string HXGetTriggerName(this TMVCWebRequest req)
        {
            return req.GetHtmxHeader(HTMXRequestHeaders.TriggerName);
        }

        public static string[] HXGetTriggeringEvent(this TMVCWebRequest req)
        {
            if (req.HasHeader(HTMXRequestHeaders.TriggeringEvent))
            {
                return req.GetHtmxHeader(HTMXRequestHeaders.TriggeringEvent)
                          .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Trim())
                          .ToArray();
            }
            return new string[0];
        }

        public static JObject HXGetTriggeringEventAsJSON(this TMVCWebRequest req)
        {
            if (req.HasHeader(HTMXRequestHeaders.TriggeringEvent))
            {
                return JObject.Parse(req.GetHtmxHeader(HTMXRequestHeaders.TriggeringEvent));
            }
            return null;
        }
    }

    #endregion

    #region WebResponse Extensions

    public static class HTMXResponseExtensions
    {
        private static readonly Dictionary<TClientEventType, string> ClientEventTypes = new Dictionary<TClientEventType, string>
        {
            { TClientEventType.Received, HTMXResponseHeaders.Trigger },
            { TClientEventType.Settled, HTMXResponseHeaders.TriggerAfterSettle },
            { TClientEventType.Swapped, HTMXResponseHeaders.TriggerAfterSwap }
        };

        private static readonly Dictionary<TSwapOption, string> SwapOptions = new Dictionary<TSwapOption, string>
        {
            { TSwapOption.InnerHTML, "innerHTML" },
            { TSwapOption.OuterHTML, "outerHTML" },
            { TSwapOption.BeforeBegin, "beforebegin" },
            { TSwapOption.AfterBegin, "afterbegin" },
            { TSwapOption.BeforeEnd, "beforeend" },
            { TSwapOption.AfterEnd, "afterend" },
            { TSwapOption.Delete, "delete" },
            { TSwapOption.None, "none" }
        };

        private static readonly Dictionary<TShowScrollType, string> ShowScrollTypes = new Dictionary<TShowScrollType, string>
        {
            { TShowScrollType.None, "" },
            { TShowScrollType.Show, "show" },
            { TShowScrollType.Scroll, "scroll" }
        };

        private static readonly Dictionary<TSwapScrollTo, string> SwapScrollTo = new Dictionary<TSwapScrollTo, string>
        {
            { TSwapScrollTo.Top, "top" },
            { TSwapScrollTo.Bottom, "bottom" }
        };

        public static TMVCWebResponse HXSetPushUrl(this TMVCWebResponse resp, string URL = "")
        {
            if (string.IsNullOrEmpty(URL))
                URL = "false";
            resp.SetCustomHeader(HTMXResponseHeaders.PushURL, URL);
            return resp;
        }

        public static TMVCWebResponse HXSetReplaceUrl(this TMVCWebResponse resp, string URL = "")
        {
            if (string.IsNullOrEmpty(URL))
                URL = "false";
            resp.SetCustomHeader(HTMXResponseHeaders.ReplaceURL, URL);
            return resp;
        }

        public static TMVCWebResponse HXSetReswap(this TMVCWebResponse resp, TSwapOption option)
        {
            return resp.HXSetReswap(option, 0, 0);
        }

        public static TMVCWebResponse HXSetReswap(this TMVCWebResponse resp, TSwapOption option, int swapDelay, int settleDelay = 20)
        {
            string modifiers = "";
            if (swapDelay > 0)
                modifiers = string.Format(CultureInfo.InvariantCulture, "swap:{0}ms ", swapDelay);
            if (settleDelay > 0 && settleDelay != 20)
                modifiers += string.Format(CultureInfo.InvariantCulture, "settle:{0}ms", settleDelay);
            if (!string.IsNullOrWhiteSpace(modifiers))
                modifiers = " " + modifiers.Trim();
            resp.SetCustomHeader(HTMXResponseHeaders.Reswap, SwapOptions[option] + modifiers);
            return resp;
        }

        public static TMVCWebResponse HXSetReswap(this TMVCWebResponse resp, TSwapOption option, TShowScrollType showScroll, TSwapScrollTo to, string selector = "")
        {
            string modifiers = "";
            if (showScroll != TShowScrollType.None)
            {
                modifiers = ShowScrollTypes[showScroll];
                if (!string.IsNullOrEmpty(selector))
                    modifiers = $"{modifiers}:{selector}";
                modifiers = $"{modifiers}:{SwapScrollTo[to]}";
            }
            resp.SetCustomHeader(HTMXResponseHeaders.Reswap, SwapOptions[option] + (string.IsNullOrEmpty(modifiers) ? "" : " " + modifiers));
            return resp;
        }

        public static TMVCWebResponse HXSetRetarget(this TMVCWebResponse resp, string selector)
        {
            resp.SetCustomHeader(HTMXResponseHeaders.Retarget, selector);
            return resp;
        }

        public static TMVCWebResponse HXTriggerClientEvent(this TMVCWebResponse resp, string name, TClientEventType after = TClientEventType.Received)
        {
            return resp.HXTriggerClientEvent(name, null, after);
        }

        public static TMVCWebResponse HXTriggerClientEvent(this TMVCWebResponse resp, string name, object parameters, TClientEventType after = TClientEventType.Received)
        {
            if (parameters != null)
            {
                var dict = new Dictionary<string, object> { { name, parameters } };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
                resp.SetCustomHeader(ClientEventTypes[after], json);
            }
            else
            {
                resp.SetCustomHeader(ClientEventTypes[after], name);
            }
            return resp;
        }

        public static TMVCWebResponse HXTriggerClientEvents(this TMVCWebResponse resp, string[] names, TClientEventType after = TClientEventType.Received)
        {
            if (names == null || names.Length == 0)
                return resp;
            string value = string.Join(", ", names);
            resp.SetCustomHeader(ClientEventTypes[after], value);
            return resp;
        }

        public static TMVCWebResponse HXTriggerClientEvents(this TMVCWebResponse resp, JObject eventsDescriptors, TClientEventType after = TClientEventType.Received)
        {
            if (eventsDescriptors == null)
                return resp;
            resp.SetCustomHeader(ClientEventTypes[after], eventsDescriptors.ToString());
            return resp;
        }

        public static TMVCWebResponse HXSetPageRefresh(this TMVCWebResponse resp, bool refresh = true)
        {
            resp.SetCustomHeader(HTMXResponseHeaders.Refresh, refresh ? "true" : "false");
            return resp;
        }

        public static TMVCWebResponse HXSetRedirect(this TMVCWebResponse resp, string path)
        {
            resp.SetCustomHeader(HTMXResponseHeaders.Redirect, path);
            return resp;
        }

        public static TMVCWebResponse HXSetLocation(this TMVCWebResponse resp, string path)
        {
            resp.SetCustomHeader(HTMXResponseHeaders.Location, path);
            return resp;
        }

        public static TMVCWebResponse HXSetReSelect(this TMVCWebResponse resp, string selector)
        {
            resp.SetCustomHeader(HTMXResponseHeaders.Reselect, selector);
            return resp;
        }
    }

    #endregion

    #region Stub MVCFramework Classes

    
    public class TMVCWebRequest
    {
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public class TMVCWebResponse
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> CustomHeaders { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public void SetCustomHeader(string name, string value)
        {
            CustomHeaders[name] = value;
        }
    }

    #endregion
}
