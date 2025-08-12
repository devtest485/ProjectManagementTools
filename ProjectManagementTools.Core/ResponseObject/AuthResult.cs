namespace ProjectManagementTools.Core.ResponseObject
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public object? Data { get; set; }
    }
}
