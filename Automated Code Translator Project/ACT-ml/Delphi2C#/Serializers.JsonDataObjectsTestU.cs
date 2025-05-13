using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using NUnit.Framework;
using MVCFramework.Serializer.Commons;
using MVCFramework.Serializer.Intf;
using MVCFramework.Serializer.JsonDataObjects;
using MVCFramework.Tests.Serializer.Intf;
using MVCFramework.Tests.Serializer.Entities;
using MVCFramework.Tests.Serializer.EntitiesModule;
using JsonDataObjects;
using MVCFramework.DataSet.Utils;

namespace MVCFramework.Tests.Serializer
{
    [TestFixture]
    public class MVCTestSerializerJsonDataObjects
    {
        private IMVCSerializer fSerializer;

        [OneTimeSetUp]
        public void SetupFixture()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator = "/";
            CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator = ":";
        }

        [SetUp]
        public void Setup()
        {
            fSerializer = new TMVCJsonDataObjectsSerializer();
            fSerializer.RegisterTypeSerializer(typeof(Stream), new TMVCStreamSerializerJsonDataObject());
            fSerializer.RegisterTypeSerializer(typeof(StringReader), new TMVCStreamSerializerJsonDataObject());
            fSerializer.RegisterTypeSerializer(typeof(MemoryStream), new TMVCStreamSerializerJsonDataObject());
            fSerializer.RegisterTypeSerializer(typeof(EntityCustom), new TMVCEntityCustomSerializerJsonDataObjects());
            fSerializer.RegisterTypeSerializer(typeof(TMVCNullable<int>), new TMVCNullableIntegerSerializerJsonDataObjects());
        }

        [TearDown]
        public void TearDown()
        {
            fSerializer = null;
        }

        [Test]
        public void TestDataSetHelpers()
        {
            TEntitiesModule dm = new TEntitiesModule(null);
            try
            {
                dm.Entity.Insert();
                dm.EntityId.AsLargeInt = 1;
                dm.EntityCode.AsInteger = 2;
                dm.EntityName.AsString = "Ezequiel Juliano Müller";
                dm.EntityBirthday.AsDateTime = DateTime.ParseExact("15/10/1987", "dd/MM/yyyy", CultureInfo.InvariantCulture);
                dm.EntityAccessDateTime.AsDateTime = DateTime.Parse("17/02/2017 16:37:50");
                dm.EntityAccessTime.AsDateTime = DateTime.ParseExact("16:40:50", "HH:mm:ss", CultureInfo.InvariantCulture);
                dm.EntityActive.AsBoolean = true;
                dm.EntitySalary.AsCurrency = 100;
                dm.EntityAmount.AsFloat = 100;
                dm.EntityBlobFld.AsString = "<html><body><h1>BLOB</h1></body></html>";
                dm.EntityGUID.AsGuid = Guid.Parse("{9386C957-5379-4370-8492-8FA464A9CF0C}");

                dm.Item.Insert();
                dm.ItemId.AsLargeInt = 1;
                dm.ItemName.AsString = "Ezequiel";
                dm.Item.Post();

                dm.Item.Insert();
                dm.ItemId.AsLargeInt = 2;
                dm.ItemName.AsString = "Juliano";
                dm.Item.Post();

                dm.Departament.Insert();
                dm.DepartamentName.AsString = "Depto1";
                dm.Departament.Post();

                string s = dm.Entity.AsJSONObject(MVCNameCaseDefault.ncAsIs, new List<string> { "Ignored" });
                Assert.AreEqual("{\"Id\":1,\"Code\":2,\"Name\":\"Ezequiel Juliano Müller\",\"Salary\":100.0,\"Birthday\":\"1987-10-15\",\"AccessDateTime\":\"2017-02-17T16:37:50.000+01:00\",\"AccessTime\":\"16:40:50\",\"Active\":true,\"Amount\":100.0,\"BlobFld\":\"PGh0bWw+PGJvZHk+PGgxPkJMT0I8L2gxPjwvYm9keT48L2h0bWw+\",\"GUID\":\"{9386C957-5379-4370-8492-8FA464A9CF0C}\"}", s);
                dm.Item.First();
                s = dm.Item.AsJSONObject(MVCNameCaseDefault.ncAsIs);
                Assert.AreEqual("{\"Id_Id\":1,\"Name_Name\":\"Ezequiel\"}", s);
                dm.Item.First();
                s = dm.Item.AsJSONObject(MVCNameCaseDefault.ncUpperCase);
                Assert.AreEqual("{\"ID\":1,\"NAME\":\"Ezequiel\"}", s);
                dm.Item.First();
                s = dm.Item.AsJSONObject(MVCNameCaseDefault.ncLowerCase);
                Assert.AreEqual("{\"id\":1,\"name\":\"ezequiel\"}", s);
            }
            finally
            {
                dm.Dispose();
            }
        }

        [Test]
        public void TestDeserializeCollection()
        {
            void CheckObjectList(IList<TNote> list)
            {
                Assert.IsTrue(list.Count == 4);
                Assert.IsTrue(list[0].Description == "Description 1");
                Assert.IsTrue(list[1].Description == "Description 2");
                Assert.IsTrue(list[2].Description == "Description 3");
                Assert.IsTrue(list[3].Description == "Description 4");
            }
            const string JSON_PROPERTIES = "[{\"Description\":\"Description 1\"},{\"Description\":\"Description 2\"},{\"Description\":\"Description 3\"},{\"Description\":\"Description 4\"}]";
            const string JSON_FIELDS = "[{\"FDescription\":\"Description 1\"},{\"FDescription\":\"Description 2\"},{\"FDescription\":\"Description 3\"},{\"FDescription\":\"Description 4\"}]";
            var list1 = new List<TNote>();
            fSerializer.DeserializeCollection(JSON_PROPERTIES, list1, typeof(TNote));
            CheckObjectList(list1);
            var list2 = new List<TNote>();
            fSerializer.DeserializeCollection(JSON_FIELDS, list2, typeof(TNote), TMVCSerializationType.stFields);
            CheckObjectList(list2);
        }

        [Test]
        public void TestDeserializeDataSet()
        {
            const string JSON = "{" +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"Ezequiel Juliano Müller\"," +
                "\"Salary\":100.0," +
                "\"Birthday\":\"1987-10-15\"," +
                "\"AccessDateTime\":\"2017-02-17 16:37:50\"," +
                "\"AccessTime\":\"16:40:50\"," +
                "\"Active\":true," +
                "\"Amount\":100.0," +
                "\"BlobFld\":\"PGh0bWw+PGJvZHk+PGgxPkJMT0I8L2gxPjwvYm9keT48L2h0bWw+\"," +
                "\"Items\":[{\"Id\":1,\"Name\":\"Ezequiel\"},{\"Id\":2,\"Name\":\"Juliano\"}]," +
                "\"Departament\":{\"Name\":\"Depto1\"}," +
                "\"GUID\":\"{9386C957-5379-4370-8492-8FA464A9CF0C}\"" +
                "}";
            const string JSON_LOWERCASE = "{\"id\":1,\"name\":\"Ezequiel Juliano Müller\"}";
            const string JSON_UPPERCASE = "{\"ID\":1,\"NAME\":\"Ezequiel Juliano Müller\"}";
            const string JSON_ASIS = "{\"Id_Id\":1,\"Name_Name\":\"Ezequiel Juliano Müller\"}";
            const string JSON_LIST = "[" +
                "{\"Id_Id\":1,\"Name_Name\":\"Ezequiel Juliano Müller\"}," +
                "{\"Id_Id\":2,\"Name_Name\":\"Ezequiel Juliano Müller\"}" +
                "]";
            var dm = new TEntitiesModule(null);
            try
            {
                dm.Entity.Insert();
                dm.EntityId.AsLargeInt = 1;
                dm.EntityCode.AsInteger = 2;
                dm.EntityName.AsString = "Ezequiel Juliano Müller";
                dm.EntityBirthday.AsDateTime = DateTime.ParseExact("15/10/1987", "dd/MM/yyyy", CultureInfo.InvariantCulture);
                dm.EntityAccessDateTime.AsDateTime = DateTime.Parse("17/02/2017 16:37:50");
                dm.EntityAccessTime.AsDateTime = DateTime.ParseExact("16:40:50", "HH:mm:ss", CultureInfo.InvariantCulture);
                dm.EntityActive.AsBoolean = true;
                dm.EntitySalary.AsCurrency = 100;
                dm.EntityAmount.AsFloat = 100;
                dm.EntityBlobFld.AsString = "<html><body><h1>BLOB</h1></body></html>";
                dm.EntityGUID.AsGuid = Guid.Parse("{9386C957-5379-4370-8492-8FA464A9CF0C}");
                dm.Item.Insert();
                dm.ItemId.AsLargeInt = 1;
                dm.ItemName.AsString = "Ezequiel";
                dm.Item.Post();
                dm.Item.Insert();
                dm.ItemId.AsLargeInt = 2;
                dm.ItemName.AsString = "Juliano";
                dm.Item.Post();
                dm.Departament.Insert();
                dm.DepartamentName.AsString = "Depto1";
                dm.Departament.Post();
                dm.Entity.Post();
                string s = fSerializer.SerializeDataSetRecord(dm.Entity, new List<string> { "Ignored" });
                Assert.AreEqual(JSON, s);
                dm.EntityLowerCase.Insert();
                dm.EntityLowerCaseId.AsLargeInt = 1;
                dm.EntityLowerCaseName.AsString = "Ezequiel Juliano Müller";
                dm.EntityLowerCase.Post();
                s = fSerializer.SerializeDataSetRecord(dm.EntityLowerCase);
                Assert.AreEqual(JSON_LOWERCASE, s);
                dm.EntityUpperCase.Insert();
                dm.EntityUpperCaseId.AsLargeInt = 1;
                dm.EntityUpperCaseName.AsString = "Ezequiel Juliano Müller";
                dm.EntityUpperCase.Post();
                s = fSerializer.SerializeDataSetRecord(dm.EntityUpperCase);
                Assert.AreEqual(JSON_UPPERCASE, s);
                dm.EntityUpperCase2.Insert();
                dm.EntityUpperCase2Id.AsLargeInt = 1;
                dm.EntityUpperCase2Name.AsString = "Ezequiel Juliano Müller";
                dm.EntityUpperCase2.Post();
                s = fSerializer.SerializeDataSetRecord(dm.EntityUpperCase2, new List<string>(), MVCNameCaseDefault.ncUpperCase);
                Assert.AreEqual(JSON_UPPERCASE, s);
                dm.EntityAsIs.Insert();
                dm.EntityAsIsId.AsLargeInt = 1;
                dm.EntityAsIsName.AsString = "Ezequiel Juliano Müller";
                dm.EntityAsIs.Post();
                s = fSerializer.SerializeDataSetRecord(dm.EntityAsIs);
                Assert.AreEqual(JSON_ASIS, s);
                dm.EntityAsIs.Append();
                dm.EntityAsIsId.AsLargeInt = 2;
                dm.EntityAsIsName.AsString = "Ezequiel Juliano Müller";
                dm.EntityAsIs.Post();
                s = fSerializer.SerializeDataSet(dm.EntityAsIs);
                Assert.AreEqual(JSON_LIST, s);
                s = fSerializer.SerializeObject(dm.EntityAsIs);
                Assert.AreEqual(JSON_LIST, s);
            }
            finally
            {
                dm.Dispose();
            }
        }

        [Test]
        public void TestSerializeDateTimeProperty()
        {
            TMyObjectWithUTC obj1 = new TMyObjectWithUTC();
            try
            {
                obj1.MyDateTime = new DateTime(2020, 11, 4, 12, 12, 12, 0);
                string ser = fSerializer.SerializeObject(obj1);
                TMyObjectWithUTC obj2 = new TMyObjectWithUTC();
                try
                {
                    fSerializer.DeserializeObject(ser, obj2);
                    Assert.IsTrue(obj1.Equals(obj2));
                }
                finally
                {
                    obj2.Dispose();
                }
            }
            finally
            {
                obj1.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeSerializeEntityWithEnums()
        {
            const string JSON = "{" +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"Daniele Teti\"," +
                "\"Color\":\"RED\"," +
                "\"MonthName\":\"January\"," +
                "\"MonthName2\":\"meFebruary\"," +
                "\"MonthOrder\":0" +
                "}";
            TEntityWithEnums o = new TEntityWithEnums();
            try
            {
                o.Id = 1;
                o.Code = 2;
                o.Name = "Daniele Teti";
                o.Color = TColorEnum.RED;
                o.MonthName = TMonthEnum.meJanuary;
                o.MonthName2 = TMonthEnum.meFebruary;
                o.MonthOrder = TMonthEnum.meJanuary;
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(JSON, s);
            }
            finally
            {
                o.Dispose();
            }
            o = new TEntityWithEnums();
            try
            {
                fSerializer.DeserializeObject(JSON, o);
                Assert.AreEqual(1, o.Id);
                Assert.AreEqual(2, o.Code);
                Assert.AreEqual("Daniele Teti", o.Name);
                Assert.AreEqual((int)TMonthEnum.meJanuary, (int)o.MonthName);
                Assert.AreEqual((int)TMonthEnum.meFebruary, (int)o.MonthName2);
                Assert.AreEqual((int)TMonthEnum.meJanuary, (int)o.MonthOrder);
                Assert.AreEqual((int)TColorEnum.RED, (int)o.Color);
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeEntityCustomMemberSerializer()
        {
            const string JSON = "{" +
                "\"Entity\":{" +
                "\"AId\":1," +
                "\"ACode\":2," +
                "\"AName\":\"Ezequiel Juliano Müller\"" +
                "}," +
                "\"Notes\":\"RXplcXVpZWwgSnVsaWFubyBN/GxsZXI=\"," +
                "\"NotesAsString\":\"Ezequiel Juliano Müller\"" +
                "}";
            TSale o = new TSale();
            try
            {
                o.Entity.Id = 1;
                o.Entity.Code = 2;
                o.Entity.Name = "Ezequiel Juliano Müller";
                o.Notes.WriteString("Ezequiel Juliano Müller");
                o.NotesAsString.WriteString("Ezequiel Juliano Müller");
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(JSON, s);
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeEntityCustomSerializer()
        {
            const string JSON = "{" +
                "\"AId\":1," +
                "\"ACode\":2," +
                "\"AName\":\"Ezequiel Juliano Müller\"" +
                "}";
            TEntityCustom o = new TEntityCustom();
            try
            {
                o.Id = 1;
                o.Code = 2;
                o.Name = "Ezequiel Juliano Müller";
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(JSON, s);
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeEntityLowerCaseNames()
        {
            const string JSON = "{" +
                "\"id\":1," +
                "\"code\":2," +
                "\"name\":\"Ezequiel Juliano Müller\"" +
                "}";
            TEntityLowerCase o = new TEntityLowerCase();
            try
            {
                o.Id = 1;
                o.Code = 2;
                o.Name = "Ezequiel Juliano Müller";
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(JSON, s);
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeEntityNameAs()
        {
            const string JSON = "{" +
                "\"Id_Id\":1," +
                "\"Code_Code\":2," +
                "\"Name_Name\":\"Ezequiel Juliano Müller\"" +
                "}";
            TEntityNameAs o = new TEntityNameAs();
            try
            {
                o.Id = 1;
                o.Code = 2;
                o.Name = "Ezequiel Juliano Müller";
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(JSON, s);
                s = fSerializer.SerializeObject(o, TMVCSerializationType.stFields);
                Assert.AreEqual(JSON, s);
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeEntitySerializationType()
        {
            const string JSON_FIELDS = "{" +
                "\"FId\":1," +
                "\"FCode\":2," +
                "\"FName\":\"Ezequiel Juliano Müller\"" +
                "}";
            const string JSON_PROPERTIES = "{" +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"Ezequiel Juliano Müller\"" +
                "}";
            TEntitySerializeFields oFields = new TEntitySerializeFields();
            try
            {
                oFields.Id = 1;
                oFields.Code = 2;
                oFields.Name = "Ezequiel Juliano Müller";
                string s = fSerializer.SerializeObject(oFields);
                Assert.AreEqual(JSON_FIELDS, s);
            }
            finally
            {
                oFields.Dispose();
            }
            TEntitySerializeProperties oProperties = new TEntitySerializeProperties();
            try
            {
                oProperties.Id = 1;
                oProperties.Code = 2;
                oProperties.Name = "Ezequiel Juliano Müller";
                string s = fSerializer.SerializeObject(oProperties);
                Assert.AreEqual(JSON_PROPERTIES, s);
            }
            finally
            {
                oProperties.Dispose();
            }
        }

        [Test]
        public void TestSerializeEntityUpperCaseNames()
        {
            const string JSON = "{" +
                "\"ID\":1," +
                "\"CODE\":2," +
                "\"NAME\":\"Ezequiel Juliano Müller\"" +
                "}";
            TEntityUpperCase o = new TEntityUpperCase();
            try
            {
                o.Id = 1;
                o.Code = 2;
                o.Name = "Ezequiel Juliano Müller";
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(JSON, s);
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeEntityWithArray()
        {
            const string JSON_WITH_ARRAY = "{" +
                "\"Id\":1," +
                "\"Names\":[\"Pedro\",\"Oliveira\"]," +
                "\"Values\":[1,2]," +
                "\"Values8\":[7,8]," +
                "\"Values64\":[3,4]," +
                "\"Booleans\":[true,false,true]" +
                "}";
            TEntityWithArray o = new TEntityWithArray();
            try
            {
                o.Id = 1;
                o.Names = new string[] { "Pedro", "Oliveira" };
                o.Values = new int[] { 1, 2 };
                o.Values8 = new byte[] { 7, 8 };
                o.Values64 = new long[] { 3, 4 };
                o.Booleans = new bool[] { true, false, true };
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(JSON_WITH_ARRAY, s);
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeListOfSomething()
        {
            TListOfSomething list = new TListOfSomething();
            try
            {
                string s = fSerializer.SerializeObject(list);
                Assert.AreEqual("[" +
                    "{\"Description\":\"Description 1\"}," +
                    "{\"Description\":\"Description 2\"}" +
                    "]", s);
                s = fSerializer.SerializeObject(list, TMVCSerializationType.stFields);
                Assert.AreEqual("[" +
                    "{\"FDescription\":\"Description 1\"}," +
                    "{\"FDescription\":\"Description 2\"}" +
                    "]", s);
            }
            finally
            {
                list.Dispose();
            }
        }

        [Test]
        public void TestSerializeListWithNulls()
        {
            TPeople people = new TPeople();
            try
            {
                TPerson person = new TPerson();
                person.Id = 1;
                person.FirstName = "Daniele";
                person.LastName = "Teti";
                people.Add(person);
                people.Add(null);
                string s = fSerializer.SerializeObject(people);
                JsonObject jObj = JsonObject.Parse(s) as JsonObject;
                Assert.IsFalse(jObj.A("List").Items[0].IsNull);
                Assert.IsTrue(jObj.A("List").Items[1].IsNull);
            }
            finally
            {
                people.Dispose();
            }
        }

        [Test]
        public void TestSerializeListWithNulls2()
        {
            TPeople people = new TPeople();
            try
            {
                TPerson person = new TPerson();
                person.Id = 1;
                person.FirstName = "Daniele";
                person.LastName = "Teti";
                people.Add(person);
                people.Add(null);
                string s = fSerializer.SerializeCollection(people);
                JsonArray jArr = JsonObject.Parse(s) as JsonArray;
                Assert.IsFalse(jArr.Items[0].IsNull);
                Assert.IsTrue(jArr.Items[1].IsNull);
            }
            finally
            {
                people.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeEntityWithInterface()
        {
            const string JSON = "{" +
                "\"Id\":1," +
                "\"Name\":\"João Antônio Duarte\"," +
                "\"ChildEntity\":{" +
                "\"Code\":10," +
                "\"Description\":\"Child Entity\"" +
                "}" +
                "}";
            IEntityWithInterface entity = new EntityWithInterface();
            entity.Id = 1;
            entity.Name = "João Antônio Duarte";
            entity.ChildEntity.Code = 10;
            entity.ChildEntity.Description = "Child Entity";
            string json = fSerializer.SerializeObject(entity);
            Assert.AreEqual(JSON, json);
            entity = new EntityWithInterface();
            fSerializer.DeserializeObject(json, entity);
            Assert.AreEqual(1, entity.Id);
            Assert.AreEqual("João Antônio Duarte", entity.Name);
            Assert.AreEqual(10, entity.ChildEntity.Code);
            Assert.AreEqual("Child Entity", entity.ChildEntity.Description);
        }

        [Test]
        public void TestSerializeDeSerializeEntityWithSet()
        {
            const string O1 = "{\"MonthsSet\":\"meJanuary,meMarch\",\"ColorsSet\":\"\"}";
            const string O2 = "{\"MonthsSet\":\"\",\"ColorsSet\":\"RED\"}";
            const string O3 = "{\"MonthsSet\":\"meJanuary,meFebruary,meMarch\",\"ColorsSet\":\"RED,GREEN,BLUE\"}";
            TEntityWithSets o = new TEntityWithSets();
            try
            {
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch };
                o.ColorsSet = new HashSet<TColorEnum>();
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O1, s);
                TEntityWithSets oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum>(), oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum>();
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O2, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum>(), oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch, TMonthEnum.meFebruary };
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O3, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meFebruary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGuid()
        {
            const string JSON = "{" +
                "\"GuidValue\":\"{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}\"," +
                "\"GuidValue2\":\"ca09dc98-85ba-46e8-aba2-117c2fa8ef25\"," +
                "\"NullableGuid\":\"{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}\"," +
                "\"NullableGuid2\":\"fa51caa7-7d48-46ba-bfde-34c1f740e066\"," +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"João Antônio\"" +
                "}";
            TEntityCustomWithGuid entity = new TEntityCustomWithGuid();
            try
            {
                entity.Id = 1;
                entity.Code = 2;
                entity.Name = "João Antônio";
                entity.GuidValue = Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}");
                entity.GuidValue2 = Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}");
                entity.NullableGuid = Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}");
                entity.NullableGuid2 = Guid.Parse("{FA51CAA7-7D48-46BA-BFDE-34C1F740E066}");
                string json = fSerializer.SerializeObject(entity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                entity.Dispose();
            }
            entity = new TEntityCustomWithGuid();
            try
            {
                fSerializer.DeserializeObject(JSON, entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual(2, entity.Code);
                Assert.AreEqual("João Antônio", entity.Name);
                Assert.AreEqual(Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}"), entity.GuidValue);
                Assert.AreEqual(Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}"), entity.GuidValue2);
                Assert.AreEqual(Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}"), entity.NullableGuid.Value);
                Assert.AreEqual(Guid.Parse("{FA51CAA7-7D48-46BA-BFDE-34C1F740E066}"), entity.NullableGuid2.Value);
            }
            finally
            {
                entity.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeMultipleGenericEntity()
        {
            const string JSON = "{" +
                "\"Code\":1," +
                "\"Description\":\"General Description\"," +
                "\"Items\":[" +
                    "{\"Description\":\"Description 01\"}," +
                    "{\"Description\":\"Description 02\"}," +
                    "{\"Description\":\"Description 03\"}," +
                    "{\"Description\":\"Description 04\"}," +
                    "{\"Description\":\"Description 05\"}" +
                "]," +
                "\"Items2\":[" +
                    "{\"Description\":\"Description2 01\"}," +
                    "{\"Description\":\"Description2 02\"}," +
                    "{\"Description\":\"Description2 03\"}," +
                    "{\"Description\":\"Description2 04\"}," +
                    "{\"Description\":\"Description2 05\"}" +
                "]" +
                "}";
            TMultipleGenericEntity<TNote, TNote> genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                genericEntity.Code = 1;
                genericEntity.Description = "General Description";
                genericEntity.Items.Add(new TNote("Description 01"));
                genericEntity.Items.Add(new TNote("Description 02"));
                genericEntity.Items.Add(new TNote("Description 03"));
                genericEntity.Items.Add(new TNote("Description 04"));
                genericEntity.Items.Add(new TNote("Description 05"));
                genericEntity.Items2.Add(new TNote("Description2 01"));
                genericEntity.Items2.Add(new TNote("Description2 02"));
                genericEntity.Items2.Add(new TNote("Description2 03"));
                genericEntity.Items2.Add(new TNote("Description2 04"));
                genericEntity.Items2.Add(new TNote("Description2 05"));
                string json = fSerializer.SerializeObject(genericEntity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                genericEntity.Dispose();
            }
            genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                fSerializer.DeserializeObject(JSON, genericEntity);
                Assert.AreEqual(1, genericEntity.Code);
                Assert.AreEqual("General Description", genericEntity.Description);
                Assert.AreEqual(5, genericEntity.Items.Count);
                Assert.AreEqual("Description 01", genericEntity.Items[0].Description);
                Assert.AreEqual("Description 02", genericEntity.Items[1].Description);
                Assert.AreEqual("Description 03", genericEntity.Items[2].Description);
                Assert.AreEqual("Description 04", genericEntity.Items[3].Description);
                Assert.AreEqual("Description 05", genericEntity.Items[4].Description);
                Assert.AreEqual(5, genericEntity.Items2.Count);
                Assert.AreEqual("Description2 01", genericEntity.Items2[0].Description);
                Assert.AreEqual("Description2 02", genericEntity.Items2[1].Description);
                Assert.AreEqual("Description2 03", genericEntity.Items2[2].Description);
                Assert.AreEqual("Description2 04", genericEntity.Items2[3].Description);
                Assert.AreEqual("Description2 05", genericEntity.Items2[4].Description);
            }
            finally
            {
                genericEntity.Dispose();
            }
        }

        [Test]
        public void TestSerializeNil()
        {
            Assert.AreEqual("null", fSerializer.SerializeObject(null));
        }

        [Test]
        public void TestStringDictionary()
        {
            TMVCStringDictionary dict = new TMVCStringDictionary();
            try
            {
                dict["prop1"] = "value1";
                dict["prop2"] = "value2";
                dict["prop3"] = "value3";
                string s = fSerializer.SerializeObject(dict);
                TMVCStringDictionary dict2 = new TMVCStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(s, dict2);
                    Assert.IsTrue(dict2.ContainsKey("prop1"));
                    Assert.IsTrue(dict2.ContainsKey("prop2"));
                    Assert.IsTrue(dict2.ContainsKey("prop3"));
                    Assert.AreEqual(dict["prop1"], dict2["prop1"]);
                    Assert.AreEqual(dict["prop2"], dict2["prop2"]);
                    Assert.AreEqual(dict["prop3"], dict2["prop3"]);
                }
                finally
                {
                    dict2.Dispose();
                }
            }
            finally
            {
                dict.Dispose();
            }
            TEntityWithStringDictionary entityDict = new TEntityWithStringDictionary();
            try
            {
                entityDict.Dict["prop1"] = "value1";
                entityDict.Dict["prop2"] = "value2";
                entityDict.Dict["prop3"] = "value3";
                string sEntity = fSerializer.SerializeObject(entityDict);
                TEntityWithStringDictionary entityDict2 = new TEntityWithStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(sEntity, entityDict2);
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop1"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop2"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop3"));
                    Assert.AreEqual(entityDict.Dict["prop1"], entityDict2.Dict["prop1"]);
                    Assert.AreEqual(entityDict.Dict["prop2"], entityDict2.Dict["prop2"]);
                    Assert.AreEqual(entityDict.Dict["prop3"], entityDict2.Dict["prop3"]);
                }
                finally
                {
                    entityDict2.Dispose();
                }
            }
            finally
            {
                entityDict.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGuid()
        {
            const string JSON = "{" +
                "\"GuidValue\":\"{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}\"," +
                "\"GuidValue2\":\"ca09dc98-85ba-46e8-aba2-117c2fa8ef25\"," +
                "\"NullableGuid\":\"{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}\"," +
                "\"NullableGuid2\":\"fa51caa7-7d48-46ba-bfde-34c1f740e066\"," +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"João Antônio\"" +
                "}";
            TEntityCustomWithGuid entity = new TEntityCustomWithGuid();
            try
            {
                entity.Id = 1;
                entity.Code = 2;
                entity.Name = "João Antônio";
                entity.GuidValue = Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}");
                entity.GuidValue2 = Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}");
                entity.NullableGuid = Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}");
                entity.NullableGuid2 = Guid.Parse("{FA51CAA7-7D48-46BA-BFDE-34C1F740E066}");
                string json = fSerializer.SerializeObject(entity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                entity.Dispose();
            }
            entity = new TEntityCustomWithGuid();
            try
            {
                fSerializer.DeserializeObject(JSON, entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual(2, entity.Code);
                Assert.AreEqual("João Antônio", entity.Name);
                Assert.AreEqual(Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}"), entity.GuidValue);
                Assert.AreEqual(Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}"), entity.GuidValue2);
                Assert.AreEqual(Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}"), entity.NullableGuid.Value);
                Assert.AreEqual(Guid.Parse("{FA51CAA7-7D48-46BA-BFDE-34C1F740E066}"), entity.NullableGuid2.Value);
            }
            finally
            {
                entity.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGenericEntity()
        {
            const string JSON = "{" +
                "\"Code\":1," +
                "\"Description\":\"General Description\"," +
                "\"Items\":[" +
                    "{\"Description\":\"Description 01\"}," +
                    "{\"Description\":\"Description 02\"}," +
                    "{\"Description\":\"Description 03\"}," +
                    "{\"Description\":\"Description 04\"}," +
                    "{\"Description\":\"Description 05\"}" +
                "]" +
                "}";
            TGenericEntity<TNote> genericEntity = new TGenericEntity<TNote>();
            try
            {
                genericEntity.Code = 1;
                genericEntity.Description = "General Description";
                genericEntity.Items.Add(new TNote("Description 01"));
                genericEntity.Items.Add(new TNote("Description 02"));
                genericEntity.Items.Add(new TNote("Description 03"));
                genericEntity.Items.Add(new TNote("Description 04"));
                genericEntity.Items.Add(new TNote("Description 05"));
                string json = fSerializer.SerializeObject(genericEntity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                genericEntity.Dispose();
            }
            genericEntity = new TGenericEntity<TNote>();
            try
            {
                fSerializer.DeserializeObject(JSON, genericEntity);
                Assert.AreEqual(1, genericEntity.Code);
                Assert.AreEqual("General Description", genericEntity.Description);
                Assert.AreEqual(5, genericEntity.Items.Count);
                Assert.AreEqual("Description 01", genericEntity.Items[0].Description);
                Assert.AreEqual("Description 02", genericEntity.Items[1].Description);
                Assert.AreEqual("Description 03", genericEntity.Items[2].Description);
                Assert.AreEqual("Description 04", genericEntity.Items[3].Description);
                Assert.AreEqual("Description 05", genericEntity.Items[4].Description);
            }
            finally
            {
                genericEntity.Dispose();
            }
        }

        [Test]
        public void TestSerializeNil()
        {
            Assert.AreEqual("null", fSerializer.SerializeObject(null));
        }

        [Test]
        public void TestStringDictionary()
        {
            TMVCStringDictionary dict = new TMVCStringDictionary();
            try
            {
                dict["prop1"] = "value1";
                dict["prop2"] = "value2";
                dict["prop3"] = "value3";
                string s = fSerializer.SerializeObject(dict);
                TMVCStringDictionary dict2 = new TMVCStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(s, dict2);
                    Assert.IsTrue(dict2.ContainsKey("prop1"));
                    Assert.IsTrue(dict2.ContainsKey("prop2"));
                    Assert.IsTrue(dict2.ContainsKey("prop3"));
                    Assert.AreEqual(dict["prop1"], dict2["prop1"]);
                    Assert.AreEqual(dict["prop2"], dict2["prop2"]);
                    Assert.AreEqual(dict["prop3"], dict2["prop3"]);
                }
                finally
                {
                    dict2.Dispose();
                }
            }
            finally
            {
                dict.Dispose();
            }
            TEntityWithStringDictionary entityDict = new TEntityWithStringDictionary();
            try
            {
                entityDict.Dict["prop1"] = "value1";
                entityDict.Dict["prop2"] = "value2";
                entityDict.Dict["prop3"] = "value3";
                string sEntity = fSerializer.SerializeObject(entityDict);
                TEntityWithStringDictionary entityDict2 = new TEntityWithStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(sEntity, entityDict2);
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop1"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop2"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop3"));
                    Assert.AreEqual(entityDict.Dict["prop1"], entityDict2.Dict["prop1"]);
                    Assert.AreEqual(entityDict.Dict["prop2"], entityDict2.Dict["prop2"]);
                    Assert.AreEqual(entityDict.Dict["prop3"], entityDict2.Dict["prop3"]);
                }
                finally
                {
                    entityDict2.Dispose();
                }
            }
            finally
            {
                entityDict.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeEntityWithInterface()
        {
            const string JSON = "{" +
                "\"Id\":1," +
                "\"Name\":\"João Antônio Duarte\"," +
                "\"ChildEntity\":{" +
                    "\"Code\":10," +
                    "\"Description\":\"Child Entity\"" +
                "}" +
                "}";
            IEntityWithInterface entity = new EntityWithInterface();
            entity.Id = 1;
            entity.Name = "João Antônio Duarte";
            entity.ChildEntity.Code = 10;
            entity.ChildEntity.Description = "Child Entity";
            string json = fSerializer.SerializeObject(entity);
            Assert.AreEqual(JSON, json);
            entity = new EntityWithInterface();
            fSerializer.DeserializeObject(json, entity);
            Assert.AreEqual(1, entity.Id);
            Assert.AreEqual("João Antônio Duarte", entity.Name);
            Assert.AreEqual(10, entity.ChildEntity.Code);
            Assert.AreEqual("Child Entity", entity.ChildEntity.Description);
        }

        [Test]
        public void TestSerializeDeSerializeEntityWithSet()
        {
            const string O1 = "{\"MonthsSet\":\"meJanuary,meMarch\",\"ColorsSet\":\"\"}";
            const string O2 = "{\"MonthsSet\":\"\",\"ColorsSet\":\"RED\"}";
            const string O3 = "{\"MonthsSet\":\"meJanuary,meFebruary,meMarch\",\"ColorsSet\":\"RED,GREEN,BLUE\"}";
            TEntityWithSets o = new TEntityWithSets();
            try
            {
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch };
                o.ColorsSet = new HashSet<TColorEnum>();
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O1, s);
                TEntityWithSets oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum>(), oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum>();
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O2, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum>(), oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch, TMonthEnum.meFebruary };
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O3, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meFebruary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGuid()
        {
            const string JSON = "{" +
                "\"GuidValue\":\"{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}\"," +
                "\"GuidValue2\":\"ca09dc98-85ba-46e8-aba2-117c2fa8ef25\"," +
                "\"NullableGuid\":\"{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}\"," +
                "\"NullableGuid2\":\"fa51caa7-7d48-46ba-bfde-34c1f740e066\"," +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"João Antônio\"" +
                "}";
            TEntityCustomWithGuid entity = new TEntityCustomWithGuid();
            try
            {
                entity.Id = 1;
                entity.Code = 2;
                entity.Name = "João Antônio";
                entity.GuidValue = Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}");
                entity.GuidValue2 = Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}");
                entity.NullableGuid = Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}");
                entity.NullableGuid2 = Guid.Parse("{FA51CAA7-7d48-46BA-BFDE-34C1F740E066}");
                string json = fSerializer.SerializeObject(entity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                entity.Dispose();
            }
            entity = new TEntityCustomWithGuid();
            try
            {
                fSerializer.DeserializeObject(JSON, entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual(2, entity.Code);
                Assert.AreEqual("João Antônio", entity.Name);
                Assert.AreEqual(Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}"), entity.GuidValue);
                Assert.AreEqual(Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}"), entity.GuidValue2);
                Assert.AreEqual(Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}"), entity.NullableGuid.Value);
                Assert.AreEqual(Guid.Parse("{FA51CAA7-7d48-46BA-BFDE-34C1F740E066}"), entity.NullableGuid2.Value);
            }
            finally
            {
                entity.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeMultipleGenericEntity()
        {
            const string JSON = "{" +
                "\"Code\":1," +
                "\"Description\":\"General Description\"," +
                "\"Items\":[" +
                    "{\"Description\":\"Description 01\"}," +
                    "{\"Description\":\"Description 02\"}," +
                    "{\"Description\":\"Description 03\"}," +
                    "{\"Description\":\"Description 04\"}," +
                    "{\"Description\":\"Description 05\"}" +
                "]," +
                "\"Items2\":[" +
                    "{\"Description\":\"Description2 01\"}," +
                    "{\"Description\":\"Description2 02\"}," +
                    "{\"Description\":\"Description2 03\"}," +
                    "{\"Description\":\"Description2 04\"}," +
                    "{\"Description\":\"Description2 05\"}" +
                "]" +
                "}";
            TMultipleGenericEntity<TNote, TNote> genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                genericEntity.Code = 1;
                genericEntity.Description = "General Description";
                genericEntity.Items.Add(new TNote("Description 01"));
                genericEntity.Items.Add(new TNote("Description 02"));
                genericEntity.Items.Add(new TNote("Description 03"));
                genericEntity.Items.Add(new TNote("Description 04"));
                genericEntity.Items.Add(new TNote("Description 05"));
                genericEntity.Items2.Add(new TNote("Description2 01"));
                genericEntity.Items2.Add(new TNote("Description2 02"));
                genericEntity.Items2.Add(new TNote("Description2 03"));
                genericEntity.Items2.Add(new TNote("Description2 04"));
                genericEntity.Items2.Add(new TNote("Description2 05"));
                string json = fSerializer.SerializeObject(genericEntity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                genericEntity.Dispose();
            }
            genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                fSerializer.DeserializeObject(JSON, genericEntity);
                Assert.AreEqual(1, genericEntity.Code);
                Assert.AreEqual("General Description", genericEntity.Description);
                Assert.AreEqual(5, genericEntity.Items.Count);
                Assert.AreEqual("Description 01", genericEntity.Items[0].Description);
                Assert.AreEqual("Description 02", genericEntity.Items[1].Description);
                Assert.AreEqual("Description 03", genericEntity.Items[2].Description);
                Assert.AreEqual("Description 04", genericEntity.Items[3].Description);
                Assert.AreEqual("Description 05", genericEntity.Items[4].Description);
                Assert.AreEqual(5, genericEntity.Items2.Count);
                Assert.AreEqual("Description2 01", genericEntity.Items2[0].Description);
                Assert.AreEqual("Description2 02", genericEntity.Items2[1].Description);
                Assert.AreEqual("Description2 03", genericEntity.Items2[2].Description);
                Assert.AreEqual("Description2 04", genericEntity.Items2[3].Description);
                Assert.AreEqual("Description2 05", genericEntity.Items2[4].Description);
            }
            finally
            {
                genericEntity.Dispose();
            }
        }

        [Test]
        public void TestSerializeNil()
        {
            Assert.AreEqual("null", fSerializer.SerializeObject(null));
        }

        [Test]
        public void TestStringDictionary()
        {
            TMVCStringDictionary dict = new TMVCStringDictionary();
            try
            {
                dict["prop1"] = "value1";
                dict["prop2"] = "value2";
                dict["prop3"] = "value3";
                string s = fSerializer.SerializeObject(dict);
                TMVCStringDictionary dict2 = new TMVCStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(s, dict2);
                    Assert.IsTrue(dict2.ContainsKey("prop1"));
                    Assert.IsTrue(dict2.ContainsKey("prop2"));
                    Assert.IsTrue(dict2.ContainsKey("prop3"));
                    Assert.AreEqual(dict["prop1"], dict2["prop1"]);
                    Assert.AreEqual(dict["prop2"], dict2["prop2"]);
                    Assert.AreEqual(dict["prop3"], dict2["prop3"]);
                }
                finally
                {
                    dict2.Dispose();
                }
            }
            finally
            {
                dict.Dispose();
            }
            TEntityWithStringDictionary entityDict = new TEntityWithStringDictionary();
            try
            {
                entityDict.Dict["prop1"] = "value1";
                entityDict.Dict["prop2"] = "value2";
                entityDict.Dict["prop3"] = "value3";
                string sEntity = fSerializer.SerializeObject(entityDict);
                TEntityWithStringDictionary entityDict2 = new TEntityWithStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(sEntity, entityDict2);
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop1"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop2"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop3"));
                    Assert.AreEqual(entityDict.Dict["prop1"], entityDict2.Dict["prop1"]);
                    Assert.AreEqual(entityDict.Dict["prop2"], entityDict2.Dict["prop2"]);
                    Assert.AreEqual(entityDict.Dict["prop3"], entityDict2.Dict["prop3"]);
                }
                finally
                {
                    entityDict2.Dispose();
                }
            }
            finally
            {
                entityDict.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeEntityWithInterface()
        {
            const string JSON = "{" +
                "\"Id\":1," +
                "\"Name\":\"João Antônio Duarte\"," +
                "\"ChildEntity\":{" +
                    "\"Code\":10," +
                    "\"Description\":\"Child Entity\"" +
                "}" +
                "}";
            IEntityWithInterface entity = new EntityWithInterface();
            entity.Id = 1;
            entity.Name = "João Antônio Duarte";
            entity.ChildEntity.Code = 10;
            entity.ChildEntity.Description = "Child Entity";
            string json = fSerializer.SerializeObject(entity);
            Assert.AreEqual(JSON, json);
            entity = new EntityWithInterface();
            fSerializer.DeserializeObject(json, entity);
            Assert.AreEqual(1, entity.Id);
            Assert.AreEqual("João Antônio Duarte", entity.Name);
            Assert.AreEqual(10, entity.ChildEntity.Code);
            Assert.AreEqual("Child Entity", entity.ChildEntity.Description);
        }

        [Test]
        public void TestSerializeDeSerializeEntityWithSet()
        {
            const string O1 = "{\"MonthsSet\":\"meJanuary,meMarch\",\"ColorsSet\":\"\"}";
            const string O2 = "{\"MonthsSet\":\"\",\"ColorsSet\":\"RED\"}";
            const string O3 = "{\"MonthsSet\":\"meJanuary,meFebruary,meMarch\",\"ColorsSet\":\"RED,GREEN,BLUE\"}";
            TEntityWithSets o = new TEntityWithSets();
            try
            {
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch };
                o.ColorsSet = new HashSet<TColorEnum>();
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O1, s);
                TEntityWithSets oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum>(), oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum>();
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O2, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum>(), oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch, TMonthEnum.meFebruary };
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O3, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meFebruary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGuid()
        {
            const string JSON = "{" +
                "\"GuidValue\":\"{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}\"," +
                "\"GuidValue2\":\"ca09dc98-85ba-46e8-aba2-117c2fa8ef25\"," +
                "\"NullableGuid\":\"{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}\"," +
                "\"NullableGuid2\":\"fa51caa7-7d48-46ba-bfde-34c1f740e066\"," +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"João Antônio\"" +
                "}";
            TEntityCustomWithGuid entity = new TEntityCustomWithGuid();
            try
            {
                entity.Id = 1;
                entity.Code = 2;
                entity.Name = "João Antônio";
                entity.GuidValue = Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}");
                entity.GuidValue2 = Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}");
                entity.NullableGuid = Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}");
                entity.NullableGuid2 = Guid.Parse("{FA51CAA7-7d48-46BA-BFDE-34C1F740E066}");
                string json = fSerializer.SerializeObject(entity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                entity.Dispose();
            }
            entity = new TEntityCustomWithGuid();
            try
            {
                fSerializer.DeserializeObject(JSON, entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual(2, entity.Code);
                Assert.AreEqual("João Antônio", entity.Name);
                Assert.AreEqual(Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}"), entity.GuidValue);
                Assert.AreEqual(Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}"), entity.GuidValue2);
                Assert.AreEqual(Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}"), entity.NullableGuid.Value);
                Assert.AreEqual(Guid.Parse("{FA51CAA7-7d48-46BA-BFDE-34C1F740E066}"), entity.NullableGuid2.Value);
            }
            finally
            {
                entity.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeMultipleGenericEntity()
        {
            const string JSON = "{" +
                "\"Code\":1," +
                "\"Description\":\"General Description\"," +
                "\"Items\":[" +
                    "{\"Description\":\"Description 01\"}," +
                    "{\"Description\":\"Description 02\"}," +
                    "{\"Description\":\"Description 03\"}," +
                    "{\"Description\":\"Description 04\"}," +
                    "{\"Description\":\"Description 05\"}" +
                "]," +
                "\"Items2\":[" +
                    "{\"Description\":\"Description2 01\"}," +
                    "{\"Description\":\"Description2 02\"}," +
                    "{\"Description\":\"Description2 03\"}," +
                    "{\"Description\":\"Description2 04\"}," +
                    "{\"Description\":\"Description2 05\"}" +
                "]" +
                "}";
            TMultipleGenericEntity<TNote, TNote> genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                genericEntity.Code = 1;
                genericEntity.Description = "General Description";
                genericEntity.Items.Add(new TNote("Description 01"));
                genericEntity.Items.Add(new TNote("Description 02"));
                genericEntity.Items.Add(new TNote("Description 03"));
                genericEntity.Items.Add(new TNote("Description 04"));
                genericEntity.Items.Add(new TNote("Description 05"));
                genericEntity.Items2.Add(new TNote("Description2 01"));
                genericEntity.Items2.Add(new TNote("Description2 02"));
                genericEntity.Items2.Add(new TNote("Description2 03"));
                genericEntity.Items2.Add(new TNote("Description2 04"));
                genericEntity.Items2.Add(new TNote("Description2 05"));
                string json = fSerializer.SerializeObject(genericEntity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                genericEntity.Dispose();
            }
            genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                fSerializer.DeserializeObject(JSON, genericEntity);
                Assert.AreEqual(1, genericEntity.Code);
                Assert.AreEqual("General Description", genericEntity.Description);
                Assert.AreEqual(5, genericEntity.Items.Count);
                Assert.AreEqual("Description 01", genericEntity.Items[0].Description);
                Assert.AreEqual("Description 02", genericEntity.Items[1].Description);
                Assert.AreEqual("Description 03", genericEntity.Items[2].Description);
                Assert.AreEqual("Description 04", genericEntity.Items[3].Description);
                Assert.AreEqual("Description 05", genericEntity.Items[4].Description);
                Assert.AreEqual(5, genericEntity.Items2.Count);
                Assert.AreEqual("Description2 01", genericEntity.Items2[0].Description);
                Assert.AreEqual("Description2 02", genericEntity.Items2[1].Description);
                Assert.AreEqual("Description2 03", genericEntity.Items2[2].Description);
                Assert.AreEqual("Description2 04", genericEntity.Items2[3].Description);
                Assert.AreEqual("Description2 05", genericEntity.Items2[4].Description);
            }
            finally
            {
                genericEntity.Dispose();
            }
        }

        [Test]
        public void TestSerializeNil()
        {
            Assert.AreEqual("null", fSerializer.SerializeObject(null));
        }

        [Test]
        public void TestStringDictionary()
        {
            TMVCStringDictionary dict = new TMVCStringDictionary();
            try
            {
                dict["prop1"] = "value1";
                dict["prop2"] = "value2";
                dict["prop3"] = "value3";
                string s = fSerializer.SerializeObject(dict);
                TMVCStringDictionary dict2 = new TMVCStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(s, dict2);
                    Assert.IsTrue(dict2.ContainsKey("prop1"));
                    Assert.IsTrue(dict2.ContainsKey("prop2"));
                    Assert.IsTrue(dict2.ContainsKey("prop3"));
                    Assert.AreEqual(dict["prop1"], dict2["prop1"]);
                    Assert.AreEqual(dict["prop2"], dict2["prop2"]);
                    Assert.AreEqual(dict["prop3"], dict2["prop3"]);
                }
                finally
                {
                    dict2.Dispose();
                }
            }
            finally
            {
                dict.Dispose();
            }
            TEntityWithStringDictionary entityDict = new TEntityWithStringDictionary();
            try
            {
                entityDict.Dict["prop1"] = "value1";
                entityDict.Dict["prop2"] = "value2";
                entityDict.Dict["prop3"] = "value3";
                string sEntity = fSerializer.SerializeObject(entityDict);
                TEntityWithStringDictionary entityDict2 = new TEntityWithStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(sEntity, entityDict2);
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop1"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop2"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop3"));
                    Assert.AreEqual(entityDict.Dict["prop1"], entityDict2.Dict["prop1"]);
                    Assert.AreEqual(entityDict.Dict["prop2"], entityDict2.Dict["prop2"]);
                    Assert.AreEqual(entityDict.Dict["prop3"], entityDict2.Dict["prop3"]);
                }
                finally
                {
                    entityDict2.Dispose();
                }
            }
            finally
            {
                entityDict.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeEntityWithInterface()
        {
            const string JSON = "{" +
                "\"Id\":1," +
                "\"Name\":\"João Antônio Duarte\"," +
                "\"ChildEntity\":{" +
                    "\"Code\":10," +
                    "\"Description\":\"Child Entity\"" +
                "}" +
                "}";
            IEntityWithInterface entity = new EntityWithInterface();
            entity.Id = 1;
            entity.Name = "João Antônio Duarte";
            entity.ChildEntity.Code = 10;
            entity.ChildEntity.Description = "Child Entity";
            string json = fSerializer.SerializeObject(entity);
            Assert.AreEqual(JSON, json);
            entity = new EntityWithInterface();
            fSerializer.DeserializeObject(json, entity);
            Assert.AreEqual(1, entity.Id);
            Assert.AreEqual("João Antônio Duarte", entity.Name);
            Assert.AreEqual(10, entity.ChildEntity.Code);
            Assert.AreEqual("Child Entity", entity.ChildEntity.Description);
        }

        [Test]
        public void TestSerializeDeSerializeEntityWithSet()
        {
            const string O1 = "{\"MonthsSet\":\"meJanuary,meMarch\",\"ColorsSet\":\"\"}";
            const string O2 = "{\"MonthsSet\":\"\",\"ColorsSet\":\"RED\"}";
            const string O3 = "{\"MonthsSet\":\"meJanuary,meFebruary,meMarch\",\"ColorsSet\":\"RED,GREEN,BLUE\"}";
            TEntityWithSets o = new TEntityWithSets();
            try
            {
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch };
                o.ColorsSet = new HashSet<TColorEnum>();
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O1, s);
                TEntityWithSets oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum>(), oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum>();
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O2, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum>(), oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch, TMonthEnum.meFebruary };
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O3, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meFebruary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGuid()
        {
            const string JSON = "{" +
                "\"GuidValue\":\"{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}\"," +
                "\"GuidValue2\":\"ca09dc98-85ba-46e8-aba2-117c2fa8ef25\"," +
                "\"NullableGuid\":\"{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}\"," +
                "\"NullableGuid2\":\"fa51caa7-7d48-46ba-bfde-34c1f740e066\"," +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"João Antônio\"" +
                "}";
            TEntityCustomWithGuid entity = new TEntityCustomWithGuid();
            try
            {
                entity.Id = 1;
                entity.Code = 2;
                entity.Name = "João Antônio";
                entity.GuidValue = Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}");
                entity.GuidValue2 = Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}");
                entity.NullableGuid = Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}");
                entity.NullableGuid2 = Guid.Parse("{FA51CAA7-7d48-46ba-bfde-34C1F740E066}");
                string json = fSerializer.SerializeObject(entity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                entity.Dispose();
            }
            entity = new TEntityCustomWithGuid();
            try
            {
                fSerializer.DeserializeObject(JSON, entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual(2, entity.Code);
                Assert.AreEqual("João Antônio", entity.Name);
                Assert.AreEqual(Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}"), entity.GuidValue);
                Assert.AreEqual(Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}"), entity.GuidValue2);
                Assert.AreEqual(Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}"), entity.NullableGuid.Value);
                Assert.AreEqual(Guid.Parse("{FA51CAA7-7d48-46ba-bfde-34C1F740E066}"), entity.NullableGuid2.Value);
            }
            finally
            {
                entity.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGenericEntity()
        {
            const string JSON = "{" +
                "\"Code\":1," +
                "\"Description\":\"General Description\"," +
                "\"Items\":[" +
                    "{\"Description\":\"Description 01\"}," +
                    "{\"Description\":\"Description 02\"}," +
                    "{\"Description\":\"Description 03\"}," +
                    "{\"Description\":\"Description 04\"}," +
                    "{\"Description\":\"Description 05\"}" +
                "]" +
                "}";
            TGenericEntity<TNote> genericEntity = new TGenericEntity<TNote>();
            try
            {
                genericEntity.Code = 1;
                genericEntity.Description = "General Description";
                genericEntity.Items.Add(new TNote("Description 01"));
                genericEntity.Items.Add(new TNote("Description 02"));
                genericEntity.Items.Add(new TNote("Description 03"));
                genericEntity.Items.Add(new TNote("Description 04"));
                genericEntity.Items.Add(new TNote("Description 05"));
                string json = fSerializer.SerializeObject(genericEntity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                genericEntity.Dispose();
            }
            genericEntity = new TGenericEntity<TNote>();
            try
            {
                fSerializer.DeserializeObject(JSON, genericEntity);
                Assert.AreEqual(1, genericEntity.Code);
                Assert.AreEqual("General Description", genericEntity.Description);
                Assert.AreEqual(5, genericEntity.Items.Count);
                Assert.AreEqual("Description 01", genericEntity.Items[0].Description);
                Assert.AreEqual("Description 02", genericEntity.Items[1].Description);
                Assert.AreEqual("Description 03", genericEntity.Items[2].Description);
                Assert.AreEqual("Description 04", genericEntity.Items[3].Description);
                Assert.AreEqual("Description 05", genericEntity.Items[4].Description);
            }
            finally
            {
                genericEntity.Dispose();
            }
        }

        [Test]
        public void TestSerializeNil()
        {
            Assert.AreEqual("null", fSerializer.SerializeObject(null));
        }

        [Test]
        public void TestStringDictionary()
        {
            TMVCStringDictionary dict = new TMVCStringDictionary();
            try
            {
                dict["prop1"] = "value1";
                dict["prop2"] = "value2";
                dict["prop3"] = "value3";
                string s = fSerializer.SerializeObject(dict);
                TMVCStringDictionary dict2 = new TMVCStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(s, dict2);
                    Assert.IsTrue(dict2.ContainsKey("prop1"));
                    Assert.IsTrue(dict2.ContainsKey("prop2"));
                    Assert.IsTrue(dict2.ContainsKey("prop3"));
                    Assert.AreEqual(dict["prop1"], dict2["prop1"]);
                    Assert.AreEqual(dict["prop2"], dict2["prop2"]);
                    Assert.AreEqual(dict["prop3"], dict2["prop3"]);
                }
                finally
                {
                    dict2.Dispose();
                }
            }
            finally
            {
                dict.Dispose();
            }
            TEntityWithStringDictionary entityDict = new TEntityWithStringDictionary();
            try
            {
                entityDict.Dict["prop1"] = "value1";
                entityDict.Dict["prop2"] = "value2";
                entityDict.Dict["prop3"] = "value3";
                string sEntity = fSerializer.SerializeObject(entityDict);
                TEntityWithStringDictionary entityDict2 = new TEntityWithStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(sEntity, entityDict2);
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop1"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop2"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop3"));
                    Assert.AreEqual(entityDict.Dict["prop1"], entityDict2.Dict["prop1"]);
                    Assert.AreEqual(entityDict.Dict["prop2"], entityDict2.Dict["prop2"]);
                    Assert.AreEqual(entityDict.Dict["prop3"], entityDict2.Dict["prop3"]);
                }
                finally
                {
                    entityDict2.Dispose();
                }
            }
            finally
            {
                entityDict.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeEntityWithInterface()
        {
            const string JSON = "{" +
                "\"Id\":1," +
                "\"Name\":\"João Antônio Duarte\"," +
                "\"ChildEntity\":{" +
                    "\"Code\":10," +
                    "\"Description\":\"Child Entity\"" +
                "}" +
                "}";
            IEntityWithInterface entity = new EntityWithInterface();
            entity.Id = 1;
            entity.Name = "João Antônio Duarte";
            entity.ChildEntity.Code = 10;
            entity.ChildEntity.Description = "Child Entity";
            string json = fSerializer.SerializeObject(entity);
            Assert.AreEqual(JSON, json);
            entity = new EntityWithInterface();
            fSerializer.DeserializeObject(json, entity);
            Assert.AreEqual(1, entity.Id);
            Assert.AreEqual("João Antônio Duarte", entity.Name);
            Assert.AreEqual(10, entity.ChildEntity.Code);
            Assert.AreEqual("Child Entity", entity.ChildEntity.Description);
        }

        [Test]
        public void TestSerializeDeSerializeEntityWithSet()
        {
            const string O1 = "{\"MonthsSet\":\"meJanuary,meMarch\",\"ColorsSet\":\"\"}";
            const string O2 = "{\"MonthsSet\":\"\",\"ColorsSet\":\"RED\"}";
            const string O3 = "{\"MonthsSet\":\"meJanuary,meFebruary,meMarch\",\"ColorsSet\":\"RED,GREEN,BLUE\"}";
            TEntityWithSets o = new TEntityWithSets();
            try
            {
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch };
                o.ColorsSet = new HashSet<TColorEnum>();
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O1, s);
                TEntityWithSets oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum>(), oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum>();
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O2, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum>(), oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch, TMonthEnum.meFebruary };
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O3, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meFebruary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGuid()
        {
            const string JSON = "{" +
                "\"GuidValue\":\"{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}\"," +
                "\"GuidValue2\":\"ca09dc98-85ba-46e8-aba2-117c2fa8ef25\"," +
                "\"NullableGuid\":\"{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}\"," +
                "\"NullableGuid2\":\"fa51caa7-7d48-46ba-bfde-34c1f740e066\"," +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"João Antônio\"" +
                "}";
            TEntityCustomWithGuid entity = new TEntityCustomWithGuid();
            try
            {
                entity.Id = 1;
                entity.Code = 2;
                entity.Name = "João Antônio";
                entity.GuidValue = Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}");
                entity.GuidValue2 = Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}");
                entity.NullableGuid = Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}");
                entity.NullableGuid2 = Guid.Parse("{FA51CAA7-7d48-46BA-BFDE-34C1F740E066}");
                string json = fSerializer.SerializeObject(entity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                entity.Dispose();
            }
            entity = new TEntityCustomWithGuid();
            try
            {
                fSerializer.DeserializeObject(JSON, entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual(2, entity.Code);
                Assert.AreEqual("João Antônio", entity.Name);
                Assert.AreEqual(Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}"), entity.GuidValue);
                Assert.AreEqual(Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}"), entity.GuidValue2);
                Assert.AreEqual(Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}"), entity.NullableGuid.Value);
                Assert.AreEqual(Guid.Parse("{FA51CAA7-7d48-46BA-BFDE-34C1F740E066}"), entity.NullableGuid2.Value);
            }
            finally
            {
                entity.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeMultipleGenericEntity()
        {
            const string JSON = "{" +
                "\"Code\":1," +
                "\"Description\":\"General Description\"," +
                "\"Items\":[" +
                    "{\"Description\":\"Description 01\"}," +
                    "{\"Description\":\"Description 02\"}," +
                    "{\"Description\":\"Description 03\"}," +
                    "{\"Description\":\"Description 04\"}," +
                    "{\"Description\":\"Description 05\"}" +
                "]," +
                "\"Items2\":[" +
                    "{\"Description\":\"Description2 01\"}," +
                    "{\"Description\":\"Description2 02\"}," +
                    "{\"Description\":\"Description2 03\"}," +
                    "{\"Description\":\"Description2 04\"}," +
                    "{\"Description\":\"Description2 05\"}" +
                "]" +
                "}";
            TMultipleGenericEntity<TNote, TNote> genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                genericEntity.Code = 1;
                genericEntity.Description = "General Description";
                genericEntity.Items.Add(new TNote("Description 01"));
                genericEntity.Items.Add(new TNote("Description 02"));
                genericEntity.Items.Add(new TNote("Description 03"));
                genericEntity.Items.Add(new TNote("Description 04"));
                genericEntity.Items.Add(new TNote("Description 05"));
                genericEntity.Items2.Add(new TNote("Description2 01"));
                genericEntity.Items2.Add(new TNote("Description2 02"));
                genericEntity.Items2.Add(new TNote("Description2 03"));
                genericEntity.Items2.Add(new TNote("Description2 04"));
                genericEntity.Items2.Add(new TNote("Description2 05"));
                string json = fSerializer.SerializeObject(genericEntity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                genericEntity.Dispose();
            }
            genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                fSerializer.DeserializeObject(JSON, genericEntity);
                Assert.AreEqual(1, genericEntity.Code);
                Assert.AreEqual("General Description", genericEntity.Description);
                Assert.AreEqual(5, genericEntity.Items.Count);
                Assert.AreEqual("Description 01", genericEntity.Items[0].Description);
                Assert.AreEqual("Description 02", genericEntity.Items[1].Description);
                Assert.AreEqual("Description 03", genericEntity.Items[2].Description);
                Assert.AreEqual("Description 04", genericEntity.Items[3].Description);
                Assert.AreEqual("Description 05", genericEntity.Items[4].Description);
                Assert.AreEqual(5, genericEntity.Items2.Count);
                Assert.AreEqual("Description2 01", genericEntity.Items2[0].Description);
                Assert.AreEqual("Description2 02", genericEntity.Items2[1].Description);
                Assert.AreEqual("Description2 03", genericEntity.Items2[2].Description);
                Assert.AreEqual("Description2 04", genericEntity.Items2[3].Description);
                Assert.AreEqual("Description2 05", genericEntity.Items2[4].Description);
            }
            finally
            {
                genericEntity.Dispose();
            }
        }

        [Test]
        public void TestSerializeNil()
        {
            Assert.AreEqual("null", fSerializer.SerializeObject(null));
        }

        [Test]
        public void TestStringDictionary()
        {
            TMVCStringDictionary dict = new TMVCStringDictionary();
            try
            {
                dict["prop1"] = "value1";
                dict["prop2"] = "value2";
                dict["prop3"] = "value3";
                string s = fSerializer.SerializeObject(dict);
                TMVCStringDictionary dict2 = new TMVCStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(s, dict2);
                    Assert.IsTrue(dict2.ContainsKey("prop1"));
                    Assert.IsTrue(dict2.ContainsKey("prop2"));
                    Assert.IsTrue(dict2.ContainsKey("prop3"));
                    Assert.AreEqual(dict["prop1"], dict2["prop1"]);
                    Assert.AreEqual(dict["prop2"], dict2["prop2"]);
                    Assert.AreEqual(dict["prop3"], dict2["prop3"]);
                }
                finally
                {
                    dict2.Dispose();
                }
            }
            finally
            {
                dict.Dispose();
            }
            TEntityWithStringDictionary entityDict = new TEntityWithStringDictionary();
            try
            {
                entityDict.Dict["prop1"] = "value1";
                entityDict.Dict["prop2"] = "value2";
                entityDict.Dict["prop3"] = "value3";
                string sEntity = fSerializer.SerializeObject(entityDict);
                TEntityWithStringDictionary entityDict2 = new TEntityWithStringDictionary();
                try
                {
                    fSerializer.DeserializeObject(sEntity, entityDict2);
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop1"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop2"));
                    Assert.IsTrue(entityDict2.Dict.ContainsKey("prop3"));
                    Assert.AreEqual(entityDict.Dict["prop1"], entityDict2.Dict["prop1"]);
                    Assert.AreEqual(entityDict.Dict["prop2"], entityDict2.Dict["prop2"]);
                    Assert.AreEqual(entityDict.Dict["prop3"], entityDict2.Dict["prop3"]);
                }
                finally
                {
                    entityDict2.Dispose();
                }
            }
            finally
            {
                entityDict.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeEntityWithInterface()
        {
            const string JSON = "{" +
                "\"Id\":1," +
                "\"Name\":\"João Antônio Duarte\"," +
                "\"ChildEntity\":{" +
                    "\"Code\":10," +
                    "\"Description\":\"Child Entity\"" +
                "}" +
                "}";
            IEntityWithInterface entity = new EntityWithInterface();
            entity.Id = 1;
            entity.Name = "João Antônio Duarte";
            entity.ChildEntity.Code = 10;
            entity.ChildEntity.Description = "Child Entity";
            string json = fSerializer.SerializeObject(entity);
            Assert.AreEqual(JSON, json);
            entity = new EntityWithInterface();
            fSerializer.DeserializeObject(json, entity);
            Assert.AreEqual(1, entity.Id);
            Assert.AreEqual("João Antônio Duarte", entity.Name);
            Assert.AreEqual(10, entity.ChildEntity.Code);
            Assert.AreEqual("Child Entity", entity.ChildEntity.Description);
        }

        [Test]
        public void TestSerializeDeSerializeEntityWithSet()
        {
            const string O1 = "{\"MonthsSet\":\"meJanuary,meMarch\",\"ColorsSet\":\"\"}";
            const string O2 = "{\"MonthsSet\":\"\",\"ColorsSet\":\"RED\"}";
            const string O3 = "{\"MonthsSet\":\"meJanuary,meFebruary,meMarch\",\"ColorsSet\":\"RED,GREEN,BLUE\"}";
            TEntityWithSets o = new TEntityWithSets();
            try
            {
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch };
                o.ColorsSet = new HashSet<TColorEnum>();
                string s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O1, s);
                TEntityWithSets oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum>(), oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum>();
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O2, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum>(), oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
                o.MonthsSet = new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meMarch, TMonthEnum.meFebruary };
                o.ColorsSet = new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE };
                s = fSerializer.SerializeObject(o);
                Assert.AreEqual(O3, s);
                oClone = new TEntityWithSets();
                try
                {
                    fSerializer.DeserializeObject(s, oClone);
                    CollectionAssert.AreEqual(new HashSet<TMonthEnum> { TMonthEnum.meJanuary, TMonthEnum.meFebruary, TMonthEnum.meMarch }, oClone.MonthsSet);
                    CollectionAssert.AreEqual(new HashSet<TColorEnum> { TColorEnum.RED, TColorEnum.GREEN, TColorEnum.BLUE }, oClone.ColorsSet);
                }
                finally
                {
                    oClone.Dispose();
                }
            }
            finally
            {
                o.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeGuid()
        {
            const string JSON = "{" +
                "\"GuidValue\":\"{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}\"," +
                "\"GuidValue2\":\"ca09dc98-85ba-46e8-aba2-117c2fa8ef25\"," +
                "\"NullableGuid\":\"{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}\"," +
                "\"NullableGuid2\":\"fa51caa7-7d48-46ba-bfde-34c1f740e066\"," +
                "\"Id\":1," +
                "\"Code\":2," +
                "\"Name\":\"João Antônio\"" +
                "}";
            TEntityCustomWithGuid entity = new TEntityCustomWithGuid();
            try
            {
                entity.Id = 1;
                entity.Code = 2;
                entity.Name = "João Antônio";
                entity.GuidValue = Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}");
                entity.GuidValue2 = Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}");
                entity.NullableGuid = Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}");
                entity.NullableGuid2 = Guid.Parse("{FA51CAA7-7d48-46BA-BFDE-34C1F740E066}");
                string json = fSerializer.SerializeObject(entity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                entity.Dispose();
            }
            entity = new TEntityCustomWithGuid();
            try
            {
                fSerializer.DeserializeObject(JSON, entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual(2, entity.Code);
                Assert.AreEqual("João Antônio", entity.Name);
                Assert.AreEqual(Guid.Parse("{AEED1A0F-9061-40F0-9FDA-D69AE7F20222}"), entity.GuidValue);
                Assert.AreEqual(Guid.Parse("{CA09DC98-85BA-46E8-ABA2-117C2FA8EF25}"), entity.GuidValue2);
                Assert.AreEqual(Guid.Parse("{EABA9B61-6812-4F0A-9469-D247EB2DA8F4}"), entity.NullableGuid.Value);
                Assert.AreEqual(Guid.Parse("{FA51CAA7-7d48-46BA-BFDE-34C1F740E066}"), entity.NullableGuid2.Value);
            }
            finally
            {
                entity.Dispose();
            }
        }

        [Test]
        public void TestSerializeDeserializeMultipleGenericEntity()
        {
            const string JSON = "{" +
                "\"Code\":1," +
                "\"Description\":\"General Description\"," +
                "\"Items\":[" +
                    "{\"Description\":\"Description 01\"}," +
                    "{\"Description\":\"Description 02\"}," +
                    "{\"Description\":\"Description 03\"}," +
                    "{\"Description\":\"Description 04\"}," +
                    "{\"Description\":\"Description 05\"}" +
                "]," +
                "\"Items2\":[" +
                    "{\"Description\":\"Description2 01\"}," +
                    "{\"Description\":\"Description2 02\"}," +
                    "{\"Description\":\"Description2 03\"}," +
                    "{\"Description\":\"Description2 04\"}," +
                    "{\"Description\":\"Description2 05\"}" +
                "]" +
                "}";
            TMultipleGenericEntity<TNote, TNote> genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                genericEntity.Code = 1;
                genericEntity.Description = "General Description";
                genericEntity.Items.Add(new TNote("Description 01"));
                genericEntity.Items.Add(new TNote("Description 02"));
                genericEntity.Items.Add(new TNote("Description 03"));
                genericEntity.Items.Add(new TNote("Description 04"));
                genericEntity.Items.Add(new TNote("Description 05"));
                genericEntity.Items2.Add(new TNote("Description2 01"));
                genericEntity.Items2.Add(new TNote("Description2 02"));
                genericEntity.Items2.Add(new TNote("Description2 03"));
                genericEntity.Items2.Add(new TNote("Description2 04"));
                genericEntity.Items2.Add(new TNote("Description2 05"));
                string json = fSerializer.SerializeObject(genericEntity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                genericEntity.Dispose();
            }
            genericEntity = new TMultipleGenericEntity<TNote, TNote>();
            try
            {
                fSerializer.DeserializeObject(JSON, genericEntity);
                Assert.AreEqual(1, genericEntity.Code);
                Assert.AreEqual("General Description", genericEntity.Description);
                Assert.AreEqual(5, genericEntity.Items.Count);
                Assert.AreEqual("Description 01", genericEntity.Items[0].Description);
                Assert.AreEqual("Description 02", genericEntity.Items[1].Description);
                Assert.AreEqual("Description 03", genericEntity.Items[2].Description);
                Assert.AreEqual("Description 04", genericEntity.Items[3].Description);
                Assert.AreEqual("Description 05", genericEntity.Items[4].Description);
                Assert.AreEqual(5, genericEntity.Items2.Count);
                Assert.AreEqual("Description2 01", genericEntity.Items2[0].Description);
                Assert.AreEqual("Description2 02", genericEntity.Items2[1].Description);
                Assert.AreEqual("Description2 03", genericEntity.Items2[2].Description);
                Assert.AreEqual("Description2 04", genericEntity.Items2[3].Description);
                Assert.AreEqual("Description2 05", genericEntity.Items2[4].Description);
            }
            finally
            {
                genericEntity.Dispose();
            }
        }

        [Test]
        public void TestSerializeNil()
        {
            Assert.AreEqual("null", fSerializer.SerializeObject(null));
        }

        [Test]
        public void TestSerializeDeserializeGenericEntity()
        {
            const string JSON = "{" +
                "\"Code\":1," +
                "\"Description\":\"General Description\"," +
                "\"Items\":[" +
                    "{\"Description\":\"Description 01\"}," +
                    "{\"Description\":\"Description 02\"}," +
                    "{\"Description\":\"Description 03\"}," +
                    "{\"Description\":\"Description 04\"}," +
                    "{\"Description\":\"Description 05\"}" +
                "]" +
                "}";
            TGenericEntity<TNote> genericEntity = new TGenericEntity<TNote>();
            try
            {
                genericEntity.Code = 1;
                genericEntity.Description = "General Description";
                genericEntity.Items.Add(new TNote("Description 01"));
                genericEntity.Items.Add(new TNote("Description 02"));
                genericEntity.Items.Add(new TNote("Description 03"));
                genericEntity.Items.Add(new TNote("Description 04"));
                genericEntity.Items.Add(new TNote("Description 05"));
                string json = fSerializer.SerializeObject(genericEntity);
                Assert.AreEqual(JSON, json);
            }
            finally
            {
                genericEntity.Dispose();
            }
            genericEntity = new TGenericEntity<TNote>();
            try
            {
                fSerializer.DeserializeObject(JSON, genericEntity);
                Assert.AreEqual(1, genericEntity.Code);
                Assert.AreEqual("General Description", genericEntity.Description);
                Assert.AreEqual(5, genericEntity.Items.Count);
                Assert.AreEqual("Description 01", genericEntity.Items[0].Description);
                Assert.AreEqual("Description 02", genericEntity.Items[1].Description);
                Assert.AreEqual("Description 03", genericEntity.Items[2].Description);
                Assert.AreEqual("Description 04", genericEntity.Items[3].Description);
                Assert.AreEqual("Description 05", genericEntity.Items[4].Description);
            }
            finally
            {
                genericEntity.Dispose();
            }
        }

        [Test]
        public void TestDoNotSerializeDoNotDeSerialize()
        {
            TPartialSerializableType obj = new TPartialSerializableType();
            try
            {
                string s = fSerializer.SerializeObject(obj);
                Assert.IsFalse(s.Contains("prop1"));
                Assert.IsTrue(s.Contains("prop2"));
                Assert.IsFalse(s.Contains("prop3"));
                Assert.IsTrue(s.Contains("prop4"));
            }
            finally
            {
                obj.Dispose();
            }
            obj = new TPartialSerializableType();
            try
            {
                fSerializer.DeserializeObject("{\"prop1\":\"x1\",\"prop2\":\"x2\",\"prop3\":\"x3\",\"prop4\":\"x4\"}", obj);
                Assert.AreEqual("x1", obj.Prop1);
                Assert.AreEqual("prop2", obj.Prop2);
                Assert.AreEqual("prop3", obj.Prop3);
                Assert.AreEqual("x4", obj.Prop4);
            }
            finally
            {
                obj.Dispose();
            }
        }

        [Test]
        public void TestIssue792()
        {
            TMyObj myObj = new TMyObj();
            try
            {
                myObj.Name = "will be changed";
                IMVCSerializer ser = new TMVCJsonDataObjectsSerializer();
                ser.DeserializeObject("{ \"dataobject\" : { \"name\" : \"Daniele\", \"number\" : 123 } }", myObj, TMVCSerializationType.stDefault, null, "dataobject");
                Assert.IsTrue(ser.SerializeObject(myObj).Contains("Daniele"));
            }
            finally
            {
                myObj.Dispose();
            }
            myObj = new TMyObj();
            try
            {
                IMVCSerializer ser = new TMVCJsonDataObjectsSerializer();
                myObj.Name = "the untouchable";
                ser.DeserializeObject("{ \"dataobject\" : null}", myObj, TMVCSerializationType.stDefault, null, "dataobject");
                Assert.IsTrue(ser.SerializeObject(myObj).Contains("the untouchable"));
            }
            finally
            {
                myObj.Dispose();
            }
        }
    }

    public class MVCTestSerializerJsonDataObjects_EntityCustomSerializer : IMVCTypeSerializer
    {
        public void Serialize(TValue elementValue, out object serializerObject, TCustomAttribute[] attributes)
        {
            TEntityCustom entity = elementValue.AsType<TEntityCustom>();
            serializerObject = new JsonObject();
            ((JsonObject)serializerObject).L["AId"] = entity.Id;
            ((JsonObject)serializerObject).I["ACode"] = entity.Code;
            ((JsonObject)serializerObject).S["AName"] = entity.Name;
        }

        public void SerializeAttribute(TValue elementValue, string propertyName, object serializerObject, TCustomAttribute[] attributes)
        {
            TEntityCustom entity = elementValue.AsType<TEntityCustom>();
            ((JsonObject)serializerObject).Object[propertyName].L["AId"] = entity.Id;
            ((JsonObject)serializerObject).Object[propertyName].I["ACode"] = entity.Code;
            ((JsonObject)serializerObject).Object[propertyName].S["AName"] = entity.Name;
        }

        public void SerializeRoot(object obj, out object serializerObject, TCustomAttribute[] attributes, TMVCSerializationAction serializationAction = null)
        {
            TEntityCustom entity = obj as TEntityCustom;
            serializerObject = new JsonObject();
            ((JsonObject)serializerObject).L["AId"] = entity.Id;
            ((JsonObject)serializerObject).I["ACode"] = entity.Code;
            ((JsonObject)serializerObject).S["AName"] = entity.Name;
        }

        public void Deserialize(object serializedObject, ref TValue elementValue, TCustomAttribute[] attributes)
        {
            JsonObject json = serializedObject as JsonObject;
            TEntityCustom entity = elementValue.AsType<TEntityCustom>();
            entity.Id = json.I["AId"];
            entity.Code = json.I["ACode"];
            entity.Name = json.S["AName"];
        }

        public void DeserializeAttribute(ref TValue elementValue, string propertyName, object serializerObject, TCustomAttribute[] attributes)
        {
            DeserializeRoot(serializerObject, ref elementValue, attributes);
        }

        public void DeserializeRoot(object serializerObject, object obj, TCustomAttribute[] attributes)
        {
            TEntityCustom entity = obj as TEntityCustom;
            JsonObject json = serializerObject as JsonObject;
            entity.Id = json.I["AId"];
            entity.Code = json.I["ACode"];
            entity.Name = json.S["AName"];
        }
    }

    public class MVCTestSerializerJsonDataObjects_NullableIntegerSerializer : IMVCTypeSerializer
    {
        public void Serialize(TValue elementValue, out object serializerObject, TCustomAttribute[] attributes)
        {
        }

        public void SerializeAttribute(TValue elementValue, string propertyName, object serializerObject, TCustomAttribute[] attributes)
        {
        }

        public void SerializeRoot(object obj, out object serializerObject, TCustomAttribute[] attributes, TMVCSerializationAction serializationAction = null)
        {
        }

        public void Deserialize(object serializerObject, ref TValue elementValue, TCustomAttribute[] attributes)
        {
        }

        public void DeserializeAttribute(ref TValue elementValue, string propertyName, object serializerObject, TCustomAttribute[] attributes)
        {
        }

        public void DeserializeRoot(object serializerObject, object obj, TCustomAttribute[] attributes)
        {
        }
    }

    public static class Extensions
    {
        public static void Dispose(this object obj)
        {
            (obj as IDisposable)?.Dispose();
        }
    }
}
