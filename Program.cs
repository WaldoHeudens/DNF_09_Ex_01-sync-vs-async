using System.Diagnostics;

// Dit voorbeeld toont het verschil tussen synchroon en asynchroon werken.
// We doen alsof we drie bestanden ophalen van een trage bron (een server).
// Elke oproep duurt ongeveer 300 ms; die vertraging simuleren we.

string[] bestanden = ["rapport.pdf", "foto.png", "data.csv"];

// --- Synchroon: de een na de ander ---
// Elke oproep blokkeert de thread tot ze klaar is. De totale tijd is de
// som van alle oproepen: drie keer ongeveer 300 ms is ongeveer 900 ms.
Console.WriteLine("== Synchroon: de een na de ander ==");
Stopwatch klok = Stopwatch.StartNew();

foreach (string naam in bestanden)
{
    int bytes = HaalOpSync(naam);
    Console.WriteLine($"  {naam}: {bytes} bytes");
}

Console.WriteLine($"Synchroon klaar na ongeveer {Rond(klok.ElapsedMilliseconds)} ms.\n");

// --- Asynchroon: alles tegelijk ---
// We starten de drie oproepen zonder er meteen op te wachten. Terwijl de
// ene wacht, kan de andere doorgaan. Task.WhenAll wacht tot ze allemaal
// klaar zijn. De totale tijd is ongeveer die van de traagste: ongeveer 300 ms.
Console.WriteLine("== Asynchroon: alles tegelijk ==");
klok.Restart();

Task<int>[] taken = bestanden.Select(HaalOpAsync).ToArray();
int[] groottes = await Task.WhenAll(taken);

for (int i = 0; i < bestanden.Length; i++)
{
    Console.WriteLine($"  {bestanden[i]}: {groottes[i]} bytes");
}

Console.WriteLine($"Asynchroon klaar na ongeveer {Rond(klok.ElapsedMilliseconds)} ms.\n");

// --- Task.Run: rekenwerk naar een andere thread ---
// Task.Delay is I/O-achtig: er is geen thread bezig tijdens het wachten.
// Zware berekeningen (CPU-werk) blokkeren wel. Die zet je met Task.Run op
// een aparte thread, zodat de hoofdthread vrij blijft.
Console.WriteLine("== Task.Run: rekenwerk op een andere thread ==");
long som = await Task.Run(BerekenZwaar);
Console.WriteLine($"De zware berekening gaf {som}.");

// De grootte hangt af van de lengte van de naam; puur ter illustratie.
static int HaalOpSync(string naam)
{
    Thread.Sleep(300);          // blokkeert de thread
    return naam.Length * 100;
}

static async Task<int> HaalOpAsync(string naam)
{
    await Task.Delay(300);      // geeft de thread vrij tijdens het wachten
    return naam.Length * 100;
}

static long BerekenZwaar()
{
    long totaal = 0;
    for (int i = 0; i < 50_000_000; i++)
    {
        totaal += i;
    }

    return totaal;
}

// Rond af op honderdtallen zodat de uitvoer stabiel blijft over machines.
static long Rond(long ms) => (long)(Math.Round(ms / 100.0) * 100);
