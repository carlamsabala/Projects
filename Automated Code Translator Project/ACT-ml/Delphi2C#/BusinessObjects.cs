using System;
using System.Collections.Generic;
using System.Linq;

namespace BusinessObjects
{
    #region Custom Attributes

    [AttributeUsage(AttributeTargets.Class)]
    public class MVCNameCaseAttribute : Attribute
    {
        public string NameCase { get; }
        public MVCNameCaseAttribute(string nameCase)
        {
            NameCase = nameCase;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MVCTableAttribute : Attribute
    {
        public string TableName { get; }
        public MVCTableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MVCTableFieldAttribute : Attribute
    {
        public string FieldName { get; }
        
        public MVCTableFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MVCOwnedAttribute : Attribute
    {
        public Type OwnedType { get; }
        public MVCOwnedAttribute() { }
        public MVCOwnedAttribute(Type ownedType)
        {
            OwnedType = ownedType;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MVCNameAsAttribute : Attribute
    {
        public string Name { get; }
        public MVCNameAsAttribute(string name)
        {
            Name = name;
        }
    }

    #endregion

    #region Base ActiveRecord Class

    public abstract class MVCActiveRecord
    {
        public virtual void OnBeforeInsertOrUpdate() { }
        public virtual void OnAfterInsertOrUpdate() { }
        public virtual void OnAfterLoad() { }
        public virtual void Store() { /* Stub: persist changes */ }
        
        public virtual void Assign(MVCActiveRecord value)
        {
            // Optionally implement a generic assignment if needed.
        }

        public static List<T> SelectRQL<T>(string query, int limit) where T : MVCActiveRecord, new()
        {
            
            return new List<T>();
        }
    }

    #endregion

    #region Business Objects

    [MVCNameCase("camelCase")]
    [MVCTable("articles")]
    public class Articles : MVCActiveRecord
    {
        [MVCTableField("id")]
        public long ID { get; set; }

        [MVCTableField("description")]
        public string Description { get; set; }

        [MVCTableField("price")]
        public int Price { get; set; }

        public Articles() : base() { }
        ~Articles() { }
    }

    [MVCNameCase("camelCase")]
    [MVCTable("order_details")]
    public class OrderDetail : MVCActiveRecord
    {
        [MVCTableField("id")]
        public long? ID { get; set; }

        [MVCTableField("id_order")]
        public long IDOrder { get; set; }

        [MVCTableField("id_article")]
        public long IDArticle { get; set; }

        private decimal _unitPrice;
        [MVCTableField("unit_price")]
        public decimal UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; RecalcTotal(); }
        }

        private int _discount;
        [MVCTableField("discount")]
        public int Discount
        {
            get => _discount;
            set { _discount = value; RecalcTotal(); }
        }

        private int _quantity;
        [MVCTableField("quantity")]
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; RecalcTotal(); }
        }

        private string _description;
        [MVCTableField("description")]
        public string Description
        {
            get => _description;
            set => _description = value;
        }

        [MVCTableField("total")]
        public decimal Total { get; private set; }

        public OrderDetail() : base() { }
        ~OrderDetail() { }

        public override void OnBeforeInsertOrUpdate()
        {
            base.OnBeforeInsertOrUpdate();
            RecalcTotal();
        }

        public void RecalcTotal()
        {
            
            Total = UnitPrice * Quantity * (1 - Discount / 100m);
        }

        public OrderDetail Clone()
        {
            var clone = new OrderDetail();
            clone.Assign(this);
            return clone;
        }

        public override void Assign(MVCActiveRecord value)
        {
            if (value is OrderDetail other)
            {
                this.ID = other.ID;
                this.IDOrder = other.IDOrder;
                this.IDArticle = other.IDArticle;
                this.UnitPrice = other.UnitPrice;
                this.Discount = other.Discount;
                this.Quantity = other.Quantity;
                this.Description = other.Description;
                
            }
            else
            {
                base.Assign(value);
            }
        }
    }

    [MVCNameCase("camelCase")]
    [MVCTable("orders")]
    public class Order : MVCActiveRecord
    {
        [MVCTableField("id")]
        public ulong? ID { get; set; }

        [MVCTableField("id_customer")]
        public int IDCustomer { get; set; }

        [MVCTableField("order_date")]
        public DateTime OrderDate { get; set; }

        [MVCTableField("total")]
        public decimal Total { get; set; }

        [MVCOwned]
        public List<OrderDetail> OrderItems { get; private set; }

        public Order() : base()
        {
            OrderItems = new List<OrderDetail>();
        }

        ~Order() { }

        public void AddOrderItem(OrderDetail orderItem)
        {
            if (ID.HasValue)
            {
                orderItem.IDOrder = (long)ID.Value;
            }
            OrderItems.Add(orderItem);
        }

        public void UpdateOrderItemByID(long orderItemID, OrderDetail orderItem)
        {
            var existing = GetOrderDetailByID(orderItemID);
            existing.Assign(orderItem);
            if (ID.HasValue)
            {
                existing.IDOrder = (long)ID.Value;
            }
        }

        public OrderDetail GetOrderDetailByID(long value)
        {
            foreach (var orderDetail in OrderItems)
            {
                if (orderDetail.ID.HasValue && orderDetail.ID.Value == value)
                {
                    return orderDetail;
                }
            }
            throw new Exception("Item not found");
        }

        public override void OnAfterInsertOrUpdate()
        {
            base.OnAfterInsertOrUpdate();
            foreach (var orderItem in OrderItems)
            {
                if (ID.HasValue)
                {
                    orderItem.IDOrder = (long)ID.Value;
                }
                orderItem.Store();
            }
        }

        public override void OnBeforeInsertOrUpdate()
        {
            base.OnBeforeInsertOrUpdate();
            RecalcTotals();
        }

        public void RecalcTotals()
        {
            Total = OrderItems.Sum(item => item.Total);
        }

        public override void OnAfterLoad()
        {
            base.OnAfterLoad();
            if (ID.HasValue)
            {
                
                var query = $"eq(idOrder,{ID.Value})";
                var list = SelectRQL<OrderDetail>(query, 1000);
                OrderItems.Clear();
                OrderItems.AddRange(list);
            }
        }
    }

    [MVCNameCase("camelCase")]
    public class OrderIn
    {
        public ulong? ID { get; set; }
        [MVCNameAs("idCustomer")]
        public ulong? IDCustomer { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? Total { get; set; }
        [MVCOwned(typeof(OrderDetail))]
        public List<OrderDetail> OrderItems { get; private set; }

        public OrderIn()
        {
            OrderItems = new List<OrderDetail>();
        }
    }

    #endregion
}
