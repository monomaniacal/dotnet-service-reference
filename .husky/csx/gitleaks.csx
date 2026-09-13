using System.ComponentModel;
using System.Diagnostics;

var startInfo = new ProcessStartInfo("gitleaks", "git --pre-commit --staged --redact")
{
    UseShellExecute = false,
};

try
{
    using var process = Process.Start(startInfo);
    process.WaitForExit();
    return process.ExitCode;
}
catch (Win32Exception)
{
    Console.WriteLine("gitleaks is not installed; skipping the local secret scan. CI still runs it.");
    Console.WriteLine("Install: https://github.com/gitleaks/gitleaks#installing");
    return 0;
}
