using AutoBloodweb;

Console.WriteLine("This is Auto Bloodweb Spender made by Yoyo. Focus window on Dead By Daylight Bloodweb now, starting in 10 seconds.");
Console.WriteLine("Press Alt + Tab switch to console and Ctrl + C to stop the application.");
Thread.Sleep(10000);

var detector = new Detector();
var spender = new BloodwebSpender(detector);

spender.Run();

