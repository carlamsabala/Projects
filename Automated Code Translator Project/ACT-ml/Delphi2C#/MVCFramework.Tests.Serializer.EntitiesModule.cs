using System;
using System.Data;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class MVCNameCaseAttribute : Attribute
{
    public string Case { get; }
    public MVCNameCaseAttribute(string nameCase)
    {
        Case = nameCase;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class MVCDoNotSerializeAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
public class MVCNameAsAttribute : Attribute
{
    public string Name { get; }
    public MVCNameAsAttribute(string name)
    {
        Name = name;
    }
}

namespace MVCFramework.Tests.Serializer
{
    
    public class EntitiesModule
    {
        public DataTable Entity { get; private set; }

        public DataTable Item { get; private set; }

        public DataTable Departament { get; private set; }

        [MVCNameCase("ncLowerCase")]
        public DataTable EntityLowerCase { get; private set; }

        [MVCNameCase("ncUpperCase")]
        public DataTable EntityUpperCase { get; private set; }

        public DataTable EntityUpperCase2 { get; private set; }

        public DataTable EntityAsIs { get; private set; }

        public EntitiesModule()
        {
            CreateDataSets();
        }

        private void CreateDataSets()
        {
            Entity = new DataTable("Entity");
            Entity.Columns.Add("EntityId", typeof(long));
            Entity.Columns.Add("EntityCode", typeof(int));
            Entity.Columns.Add("EntityName", typeof(string));
            Entity.Columns.Add("EntitySalary", typeof(decimal)); // Currency
            Entity.Columns.Add("EntityBirthday", typeof(DateTime));
            Entity.Columns.Add("EntityAccessDateTime", typeof(DateTime));
            Entity.Columns.Add("EntityAccessTime", typeof(TimeSpan));
            Entity.Columns.Add("EntityActive", typeof(bool));
            Entity.Columns.Add("EntityAmount", typeof(double));
            Entity.Columns.Add("EntityIgnored", typeof(string));
            Entity.Columns.Add("EntityIgnoredAtt", typeof(string));
            Entity.Columns.Add("EntityBlobFld", typeof(byte[]));
            Entity.Columns.Add("EntityGUID", typeof(Guid));

            Item = new DataTable("Item");
            Item.Columns.Add("ItemId", typeof(long));
            Item.Columns.Add("ItemName", typeof(string));

            Departament = new DataTable("Departament");
            Departament.Columns.Add("DepartamentName", typeof(string));

            EntityLowerCase = new DataTable("EntityLowerCase");
            EntityLowerCase.Columns.Add("entitylowercaseid", typeof(long));
            EntityLowerCase.Columns.Add("entitylowercasename", typeof(string));

            EntityUpperCase = new DataTable("EntityUpperCase");
            EntityUpperCase.Columns.Add("ENTITYUPPERCASEID", typeof(long));
            EntityUpperCase.Columns.Add("ENTITYUPPERCASENAME", typeof(string));

            EntityUpperCase2 = new DataTable("EntityUpperCase2");
            EntityUpperCase2.Columns.Add("EntityUpperCase2Id", typeof(long));
            EntityUpperCase2.Columns.Add("EntityUpperCase2Name", typeof(string));

            EntityAsIs = new DataTable("EntityAsIs");
            EntityAsIs.Columns.Add("Id_Id", typeof(long));
            EntityAsIs.Columns.Add("Name_Name", typeof(string));

            
        }
    }
}
