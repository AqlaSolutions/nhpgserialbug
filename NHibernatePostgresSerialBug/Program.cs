using System;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

public abstract class EntityBase
{
    public virtual int Id { get; set; }
}

public class TestEntity : EntityBase
{
    public override int Id { get; set; }
    public virtual string SomeField { get; set; }
}

public class AutomappingConfiguration : DefaultAutomappingConfiguration
{
    public override bool ShouldMap(Type type)
    {
        return !type.IsAbstract && type.IsSubclassOf(typeof(EntityBase));
    }

    public override bool ShouldMap(FluentNHibernate.Member member)
    {
        if (member.IsProperty && !member.CanWrite) return false;
        return base.ShouldMap(member);
    }
}

public class Program
{
    public static void Main()
    {
        var configuration = new Configuration();

        configuration.Configure("hibernate.cfg.xml");

        FluentConfiguration cfg = Fluently.Configure(configuration)
                                          .Mappings(m => m.AutoMappings.Add(
                                              AutoMap.AssemblyOf<Program>(new AutomappingConfiguration())
                                              .Conventions.AddFromAssemblyOf<Program>()
                                              .UseOverridesFromAssemblyOf<Program>()
                                                      ));

        Configuration config = cfg.BuildConfiguration();
        var schema = new SchemaExport(config);
        schema.SetOutputFile(@"test_mapping_sql.txt");
        schema.Create(true, false);
        Console.ReadLine();
    }
}


public class TestEntityOverride : IAutoMappingOverride<TestEntity>
{
    public virtual void Override(AutoMapping<TestEntity> mapping)
    {
        mapping.Id(x => x.Id)
               .CustomType<int>()
               .CustomSqlType("integer")
               .GeneratedBy.Assigned()
               .UnsavedValue(0);
    }
}