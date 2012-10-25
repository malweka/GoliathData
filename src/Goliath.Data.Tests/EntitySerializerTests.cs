using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using WebZoo.Data;

namespace Goliath.Data.Tests
{
    using DataAccess;
    using Mapping;

     [TestFixture]
    public class EntitySerializerTests
    {
         [Test]
         public void CreateInstance_with_correct_entitymap_should_create_instance()
         {
             var dbsettings = SessionHelper.Factory.DbSettings;
             var entitySerializer = new EntitySerializer(dbsettings);

             Zoo zoo = entitySerializer.CreateInstance<Zoo>();
             Assert.IsNotNull(zoo);
         }

         [Test]
         public void CreateInstance_with_correct_entitymap_should_create_list_for_many_to_many_relations()
         {
             var dbsettings = SessionHelper.Factory.DbSettings;
             var entitySerializer = new EntitySerializer(dbsettings);

             Employee emp = entitySerializer.CreateInstance<Employee>();
             Assert.IsTrue(((Collections.TrackableList<Animal>)emp.AnimalsOnAnimalsHandler_EmployeeId).IsTracking);
         }
    }
}
