using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomapperRecursiveMapping
{
    public class Source
    {
        public string Name;
        public IQueryable<Source> NestedElements;
    }
    public class Dto
    {
        public string Title;
        public ICollection<Dto> Nodes;
    }

    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestMethod1() {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Dto>()
                    .ForMember(dto => dto.Title, conf => conf.MapFrom(src => src.Name))
                    .ForMember(dto => dto.Nodes, conf => { conf.MapFrom(src => src.NestedElements); conf.ExplicitExpansion(); });
            });
            Mapper.Configuration.CompileMappings();
            Mapper.AssertConfigurationIsValid();

            IQueryable<Source> iq = new List<Source>
            {
                new Source
                {
                    Name = "TOP LEVEL",
                    NestedElements = new List<Source>()
                    {
                        new Source() { Name = "1.1" },
                        new Source() { Name = "1.2" },
                    }.AsQueryable(),
                }
            }.AsQueryable();

            var projectTo = iq.ProjectTo<Dto>(_ => _.Nodes);
            Assert.AreEqual(1, projectTo.Count());
            var first = projectTo.First();
            Assert.IsNotNull(first.Nodes);
            Assert.AreEqual(2, first.Nodes.Count());
            Assert.AreEqual("1.1", first.Nodes.First().Title);
        }
    }
}
