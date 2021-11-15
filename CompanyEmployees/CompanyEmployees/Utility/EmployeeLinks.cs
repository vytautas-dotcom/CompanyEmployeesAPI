using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.Models.LinkModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers; //
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CompanyEmployees.Utility
{
    public class EmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDto> dataShaper)
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }
        public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDto> employeeDtos,
                                             string fields,
                                             Guid companyId,
                                             HttpContext httpContext)
        {
            var shapedEmployees = ShapeData(employeeDtos, fields);
            if (ShouldGenerateLinks(httpContext))
            {
                return ReturnLinkedEmployees(employeeDtos, fields, companyId, httpContext, shapedEmployees);
            }
            return ReturnShapedEmployees(shapedEmployees);
        }
        private List<Entity> ShapeData(IEnumerable<EmployeeDto> employeeDtos, string fields)
            =>
                _dataShaper.ShapeData(employeeDtos, fields)
                           .Select(e => e.Entity)
                           .ToList();
        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            var mediaType = (MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];
            return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
        }

        private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmployees)
            =>
                new LinkResponse { ShapedEntities = shapedEmployees };

        private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDto> employeeDtos, 
                                                               string fields, 
                                                               Guid companyId,
                                                               HttpContext httpContext,
                                                               List<Entity> shapedEmployees)
        {
            var employeesList = employeeDtos.ToList();

            for (var index = 0; index < employeesList.Count(); index++)
            {
                var employeeLinks = CreateLinksForEmployee(httpContext, companyId, employeesList[index].Id, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }
            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployees(httpContext, employeeCollection);

            return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext, Guid companyId, Guid employeeId, string fields = "")
        {
            var links = new List<Link>
            {
                new Link(
                    href: _linkGenerator.GetUriByAction(httpContext, "GetEmployeeForCompany", values: new { companyId, employeeId, fields}),
                    rel: "self",
                    method: "GET"),
                new Link(
                    href: _linkGenerator.GetUriByAction(httpContext, "DeleteEmployeeForCompany", values: new { companyId, employeeId}),
                    rel: "delete_employee",
                    method: "DELETE"),
                new Link(
                    href: _linkGenerator.GetUriByAction(httpContext, "UpdateEmployeeForCompany", values: new { companyId, employeeId}),
                    rel: "update_employee",
                    method: "PUT"),
                new Link(
                    href: _linkGenerator.GetUriByAction(httpContext, "PartiallyUpdateEmployeeForCompany", values: new { companyId, employeeId}),
                    rel: "partially_update_employee",
                    method: "PATCH")
            };
            return links;
        }
        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<Entity> collectionWrapper)
        {
            collectionWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeesForCompany", values: new { }), "self", "GET"));

            return collectionWrapper;
        }
    }
}
