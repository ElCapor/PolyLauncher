namespace PolyLauncher.Models
{
    public class LaunchArguments
    {
        public string Type { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Map { get; set; } = string.Empty;

        public bool IsValid => !string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(Token);
        public bool IsClient => Type == "client" || Type == "clientbeta";
        public bool IsTest => Type == "test" || Type == "testbeta";
        public bool IsCreator => Type == "creator" || Type == "creatorbeta";
        public bool IsBeta => Type == "clientbeta" || Type == "testbeta" || Type == "creatorbeta";
        public string Release => IsBeta ? "Beta" : "Stable";
    }
}
