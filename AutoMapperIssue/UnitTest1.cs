using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoMapperIssue 
{
// http://stackoverflow.com/questions/3348634/automapper-and-inheritance-how-to-map
    using NameSourceType = System.String; using NameDtoType = System.String;
    using DescSourceType = System.Double; using DescDtoType = System.Nullable<System.Double>;
    
    public class Source
    {
        public NameSourceType Name;
        public DescSourceType Desc;
    }
    public class DtoBase              { public NameDtoType Name { get; set; } }
    public class DtoDerived : DtoBase { public DescDtoType Desc { get; set; } }
    [TestClass] public class UnitTest1
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, DtoBase>()
                    .ForMember(dto => dto.Name, conf => { // line 1-1
                        conf.MapFrom(src => src.Name);    // line 1-2
                        conf.ExplicitExpansion();         // line 1-3
                    })                                    // line 1-4
                    .Include<Source, DtoDerived>();
                cfg.CreateMap<Source, DtoDerived>()
                    // place 2-1
                    .ForMember(dto => dto.Desc, conf => {
                        conf.MapFrom(src => src.Desc);
                        conf.ExplicitExpansion();
                    })
                ;
            });
            Mapper.Configuration.CompileMappings(); // Seal();
            Mapper.AssertConfigurationIsValid();
        }

        private static readonly IQueryable<Source> _iq = new List<Source> {
            new Source() { Name = (-1).ToString(), Desc = -2,},
        } .AsQueryable();

        private static readonly Source _iqf = _iq.First();

        [TestMethod] public void ProjectAll() 
        {
            var projectTo = _iq.ProjectTo<DtoDerived>(_ => _.Name, _ => _.Desc);
            Assert.AreEqual(1, projectTo.Count()); var first = projectTo.First();
            Assert.IsNotNull(first.Desc); Assert.AreEqual(_iqf.Desc, first.Desc);
            Assert.IsNotNull(first.Name); Assert.AreEqual(_iqf.Name, first.Name);
        }
        [TestMethod] public void SkipDerived() 
        {
            var projectTo = _iq.ProjectTo<DtoDerived>(_ => _.Name);
            Assert.AreEqual(1, projectTo.Count()); var first = projectTo.First();
            Assert.IsNull(first.Desc);
            Assert.IsNotNull(first.Name); Assert.AreEqual(_iqf.Name, first.Name);
            
        }
        [TestMethod] public void SkipBase_Fail() 
        {
            var projectTo = _iq.ProjectTo<DtoDerived>(_ => _.Desc);
            Assert.AreEqual(1, projectTo.Count()); var first = projectTo.First();
            Assert.IsNotNull(first.Desc); Assert.AreEqual(_iqf.Desc, first.Desc);
            Assert.IsNull(first.Name, "Fails here. Why?");
        }
        [TestMethod] public void SkipAll_Fail() 
        {
            var projectTo = _iq.ProjectTo<DtoDerived>();
            Assert.AreEqual(1, projectTo.Count()); var first = projectTo.First();
            Assert.IsNull(first.Desc);
            Assert.IsNull(first.Name, "Fails here. Why?");
        }
    }
}

