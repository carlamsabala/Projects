using System;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BaseControllerU
{    public class BaseDataModule : IDisposable
    {
        public DbConnection Conn { get; private set; }

        public BaseDataModule()
        {
            
            Conn = null; 
        }

        public void Dispose()
        {
            if (Conn != null)
            {
                Conn.Dispose();
                Conn = null;
            }
        }
    }

    
    public class BaseController : Controller
    {
        private BaseDataModule _common;

        
        protected BaseDataModule CommonModule()
        {
            if (_common == null)
            {
                _common = new BaseDataModule();
            }
            return _common;
        }

        
        protected DbConnection Connection()
        {
            return CommonModule().Conn;
        }

        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
        }

        
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            
            if (_common != null)
            {
                _common.Dispose();
                _common = null;
            }
        }

    }
}
