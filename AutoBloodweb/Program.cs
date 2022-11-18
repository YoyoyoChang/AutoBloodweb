// See https://aka.ms/new-console-template for more information
using AutoBloodweb;

Console.WriteLine("This is auto bloodweb clicker made by Yoyo. Focus window on bloodweb, starting in 5 seconds.");
Console.WriteLine("Press Alt + Tab switch to console and Ctrl + C to stop the application.");
Thread.Sleep(5000);
var spender = new BloodwebClicker();
spender.Run();

