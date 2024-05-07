namespace app_server;
public static class Utils {
    public static string GetIp(this HttpRequest request) {
        if (request.Headers.TryGetValue("X-Forwarded-For", out var value)) {
            return value.Last();
        }
        if (request.Headers.TryGetValue("remote_addr", out var value2)) {
            return value2.Last();
        }

        return request.Host.Host; //+ request.Host.Port; //we dont want a port
    }
}
