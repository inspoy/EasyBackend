using EasyBackend.Http;
using EasyBackend.Routing;

namespace EasyBackend.Utils;

public class StartOption
{
    public Router Router { get; set; }

    public static StartOption CreateSimple()
    {
        var option = new StartOption();
        var router = new Router();
        router.AddHandler("OPTION", "*", (req, res) =>
        {
            res.SetHeader("Access-Control-Allow-Origin", "*");
            res.SetHeader("Access-Control-Allow-Headers", "*");
            res.SetHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTION");
            res.InitSimple(ResponseErrCode.Success, "OK");
            return Task.CompletedTask;
        });
        router.AddHandler("GET|POST|PUT|PATCH|DELETE", "*", (req, res) =>
        {
            res.InitSimple(ResponseErrCode.NotFound, "No handler for this request :-)");
            return Task.CompletedTask;
        });
        option.Router = router;
        return option;
    }
}
