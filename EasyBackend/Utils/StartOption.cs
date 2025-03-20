using EasyBackend.Http;
using EasyBackend.Routing;

namespace EasyBackend.Utils;

public class StartOption
{
    public string Host { get; set; }
    public int Port { get; set; }
    public Router Router { get; set; }

    public static StartOption CreateSimple(AppConfig config)
    {
        var option = new StartOption
        {
            Host = config.Host,
            Port = config.Port
        };
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

    public StartOption WithWelcome(string message)
    {
        Router.AddHandler("GET", "/", (req, res) =>
        {
            res.InitSimple(ResponseErrCode.Success, message);
            return Task.CompletedTask;
        });
        return this;
    }

    public StartOption WithPing()
    {
        Router.AddHandler("GET", "/ping", (req, res) =>
        {
            res.InitSimple(ResponseErrCode.Success, "pong");
            return Task.CompletedTask;
        });
        return this;
    }

    public StartOption WithReload(AppConfigReload cfg, Action reloadAction)
    {
        if (cfg == null || !cfg.Enabled) return this;
        var handler = Router.AddHandler("POST", cfg.Path, (req, res) =>
        {
            reloadAction?.Invoke();
            res.InitSimple(ResponseErrCode.Success, "Config reloaded");
            return Task.CompletedTask;
        });
        handler.AddMiddleware(new AuthMiddleware(cfg.Token), (newCfg, m) => { m.AuthValue = newCfg.Reload.Token; });
        return this;
    }
}
