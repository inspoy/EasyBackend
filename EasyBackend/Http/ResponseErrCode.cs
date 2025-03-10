namespace EasyBackend.Http;

public enum ResponseErrCode
{
    Success = 0,
    InvalidRequest = 1,
    InvalidToken = 2,
    ServerError = 3,
    NotImplement = 4,
    ThirdPartyError = 5,
}
