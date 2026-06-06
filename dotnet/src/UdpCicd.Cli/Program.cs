// Entry point for the `udp-cicd` CLI — Project definition for Microsoft Unified Data Platform.
using UdpCicd.Cli;

return CliApp.BuildRootCommand().Parse(args).Invoke();
