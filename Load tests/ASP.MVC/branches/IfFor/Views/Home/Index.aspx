<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%= Html.Encode(ViewData["Message"])%></h2>
 
    <h2><%
                
                    
            for(int i=0;i<5;i++)
            {
                         if (Object.Equals(Html.Encode(ViewData["number"]), "1"))
                         {
                             Response.Write("EQUAL");
                         }
            }
                 
    %></h2>
    <p>
        To learn more about ASP.NET MVC visit <a href="http://asp.net/mvc" title="ASP.NET MVC Website">http://asp.net/mvc</a>.
    <img alt="description" id ="imgWhale" src = "/MvcApplication_Simple/Content/Humpback Whale.jpg" />
    </p>
   
</asp:Content>
