
class Program
{
    static void Main(string[] args)
    {
        FuelType electricity = new FuelType("electricity", 0f);
        FuelType oil = new FuelType("oil", 5f);
        FuelContainer battery = new FuelContainer("battery", 50f, 200f, electricity);
        FuelContainer tank = new FuelContainer("tank", 20f, 100f, oil);
        Reactor mainReactor = new Reactor("NianCat Mk.1 main reactor", 1000f, 5000f, 10f, new List<FuelContainer> { tank });
        AmmoType commonShell = new AmmoType("shell 30m", 5f, 240f, 75f);
        Magazine noMagCommonShell = new Magazine("no magazine for common shell 30m", 0f, 1, commonShell, 1);
        Barrel commonBarrel = new Barrel("30m cannon", 20f, new List<AmmoType> { commonShell }, 30f);
        Turret commonTurret = new Turret("turret for 30m cannon", 50f, new List<Barrel> { commonBarrel }, 1f, true, false, false, 50, noMagCommonShell, 7f);
        LandingGear rearLandingGear1 = new LandingGear("strong rear landing gear", 50f, 2500f, 30f, 7f);
        LandingGear rearLandingGear2 = rearLandingGear1.DeepCopy();
        LandingGear frontLandingGear = new LandingGear("medium front landing gear", 35f, 750f, 25f, 4f);
        Ship commonShip = new Ship("rainbow ship", 4000f,
        new List<Reactor> { mainReactor },
        new List<FuelContainer> { battery, tank },
        new List<LandingGear> { rearLandingGear1, rearLandingGear2, frontLandingGear },
        new List<Turret> { commonTurret });

    }
}