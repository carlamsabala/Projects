using System;
using System.Collections.Generic;
using MVCFramework;
using MVCFramework.Commons;

namespace TestControllers
{
    [MVCPath("/")]
    public class SimpleController : MVCController
    {
        private List<string> _calledActions;

        private void AddCall(string actionName)
        {
            _calledActions.Add(actionName);
        }

        protected override void MVCControllerAfterCreate()
        {
            base.MVCControllerAfterCreate();
            _calledActions = new List<string>();
        }

        protected override void MVCControllerBeforeDestroy()
        {
            _calledActions.Clear();
            base.MVCControllerBeforeDestroy();
        }

        [MVCPath("/")]
        public void Index(TWebContext context)
        {
            AddCall("Index");
        }

        [MVCPath("/orders")]
        [MVCProduces("application/json")]
        public void OrdersProduceJSON(TWebContext context)
        {
        }

        [MVCPath("/orders")]
        public void Orders(TWebContext context)
        {
        }

        [MVCHTTPMethod("GET")]
        [MVCPath("/orders/($ordernumber)")]
        public void OrderNumber(TWebContext context)
        {
        }

        [MVCHTTPMethod("POST,PUT")]
        [MVCPath("/orders/($ordernumber)")]
        public void UpdateOrderNumber(TWebContext context)
        {
        }

        [MVCHTTPMethod("PATCH")]
        [MVCPath("/orders/($ordernumber)")]
        public void PatchOrder(TWebContext context)
        {
        }

        [MVCHTTPMethod("GET")]
        [MVCPath("/patient/$match")]
        public void GetOrderIssue513()
        {
            AddCall("GetOrderIssue513");
        }

        [MVCHTTPMethod("GET")]
        [MVCPath("/patient/$match/($par1)/($par2)")]
        public void GetOrderIssue513WithPars(string par1, string par2)
        {
            AddCall("GetOrderIssue513WithPars");
        }

        public List<string> CalledActions
        {
            get { return _calledActions; }
        }
    }

    public class NotSoSimpleController : MVCController
    {
        public void Method1(TWebContext ctx)
        {
        }
    }
}
