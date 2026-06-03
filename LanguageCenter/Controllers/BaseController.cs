using System.Web.Mvc;

public class BaseController : Controller
{
    protected override void OnActionExecuting(
        ActionExecutingContext filterContext)
    {
        if (Session["UserId"] == null)
        {
            filterContext.Result =
                new RedirectResult("/Account/Login");
        }

        base.OnActionExecuting(filterContext);
    }
    public class AdminController : BaseController
    {
    }
    public class TeacherController : BaseController
    {
    }
    public class StudentController : BaseController
    {
    }
}