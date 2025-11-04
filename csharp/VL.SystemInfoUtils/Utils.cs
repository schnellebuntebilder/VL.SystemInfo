// For examples, see:
// https://thegraybook.vvvv.org/reference/extending/writing-nodes.html#examples
using SharpDX.DXGI;
using System;
using System.Diagnostics;


namespace SystemInfo;

public static class Utils
{
    public static string MemInfo()
    {
        // Holt den aktuellen Prozess
        Process currentProcess = Process.GetCurrentProcess();

        // Die Refresh-Methode stellt sicher, dass die Werte aktuell sind.
        currentProcess.Refresh();

        // WorkingSet64: Die Menge des physischen Speichers (RAM), die dem Prozess
        // aktuell zugewiesen ist. Dies ist der beste Wert für "genutzter RAM".
        long usedRam = currentProcess.WorkingSet64;

        // PrivateMemorySize64: Die Menge des Speichers, die der Prozess für sich
        // reserviert hat und nicht mit anderen Prozessen teilt. Ein Teil davon
        // kann in die Auslagerungsdatei geschrieben sein.
        long privateBytes = currentProcess.PrivateMemorySize64;

        // VirtualMemorySize64: Die Größe des virtuellen Adressraums, den der
        // Prozess verwendet.
        long virtualMemory = currentProcess.VirtualMemorySize64;


        //Console.WriteLine("--- Prozess-RAM-Informationen (via System.Diagnostics.Process) ---");
        //Console.WriteLine($"Prozessname: {currentProcess.ProcessName}");
        //Console.WriteLine($"Genutzter physischer RAM (Working Set): {usedRam / 1024 / 1024} MB");
        //Console.WriteLine($"Privater Speicher: {privateBytes / 1024 / 1024} MB");
        //Console.WriteLine($"Virtueller Speicher: {virtualMemory / 1024 / 1024} MB");

        currentProcess.Dispose();

        return $"Used RAM: {usedRam / 1024 / 1024} MB, Private Bytes: {privateBytes / 1024 / 1024} MB, Virtual Memory: {virtualMemory / 1024 / 1024} MB";

        
    }

    public static string GPUMem()
    {
        // Erstellt eine Factory, um DXGI-Objekte aufzuzählen
        using (var factory = new Factory1())
        {
            // Holt den ersten Grafikadapter (GPU)
            using (var adapter = factory.GetAdapter1(0))
            {
                //Console.WriteLine($"Adapter: {adapter.Description.Description}");

                // Versucht, die Adapter3-Schnittstelle abzufragen
                try
                {
                    using (var adapter3 = adapter.QueryInterface<Adapter3>())
                    {
                        // Abfrage für den lokalen Speicher (dedizierte GPU-VRAM)
                        // Der erste Parameter '0' ist der Index des GPU-Knotens.
                        var memoryInfo = adapter3.QueryVideoMemoryInfo(0, MemorySegmentGroup.Local);

                        // Das Budget, das das Betriebssystem dem Prozess zur Verfügung stellt.
                        long budget = memoryInfo.Budget;

                        // Die aktuelle Speichernutzung durch diesen Prozess.
                        long currentUsage = memoryInfo.CurrentUsage;

                        // Console.WriteLine("\n--- Prozessspeicher-Informationen (via Adapter3) ---");
                        // Console.WriteLine($"Budget für diesen Prozess: {budget / 1024 / 1024} MB");
                        // Console.WriteLine($"Aktuelle Nutzung durch diesen Prozess: {currentUsage / 1024 / 1024} MB");
                        // Console.WriteLine($"Verfügbar für diesen Prozess: {(budget - currentUsage) / 1024 / 1024} MB");

                        return $"VRAM: {budget / 1024 / 1024} MB, in Use: {currentUsage / 1024 / 1024} MB, Available: {(budget - currentUsage) / 1024 / 1024} MB";
                    }
                }
                catch (SharpDX.SharpDXException)
                {
                    // Dieser Fehler tritt auf, wenn das System Adapter3 nicht unterstützt.
                    return "Adapter3 not available";
                }
            }
        }
    }
}