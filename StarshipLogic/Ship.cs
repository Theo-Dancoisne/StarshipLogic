using System;
using System.Linq;
using System.Collections.Generic;
//using System.Text.Json;		// near future to print in json file


public class Unit_Mass
{
	public bool isReadOnly = false;
	private float _mass;
	// checkout 'required' attribute
	public float Mass
	{
		get { return this._mass; }
		set
		{
			if (isReadOnly) throw new InvalidOperationException("This value has been set to read-only.\tYou can change this at any time.");
			else this._mass = value;
		}
		/*set {
			if (value > 0) _mass = value;
			else Console.WriteLine("Mass must be greater than 0.");
		}*/
	}

	public Unit_Mass()
	{
		this.Mass = 0;
		this.isReadOnly = true;
	}
	public Unit_Mass(float mass)
	{
		this.Mass = mass;
	}
}

public interface MassCalculations
{
	float CalculateTotalMass();
	//bool IsMassConsistent();
}

public class UsefulPhysicalProperties
{
	// weight (N) = mass (Kg) * gravity (N/kg)
	// alt0 = sea level
	
	public const double universalConstantOfPerfectGases = 8.31446261815;	// J K^(-1) mol^(-1)
	
	public const double earthGravity = 9.8;
	public const double earthAirPressure_alt0 = 1013.25;	// hPa hectopascal
	public const double earthMolarMassOfAtmosphereGases = 0.0289644;	// Kg/mol	this is the average value between all gases in the earth atmosphere
	public const double earthAbsoluteTemperature_alt0 = 288;	// Kelvin
	
	public double GetAirPressure(double altitude = 0, double gravity = earthGravity, 
	double airPressure_alt0 = earthAirPressure_alt0, double absoluteTemperature = earthAbsoluteTemperature_alt0, 
	double molarMassOfAtmosphereGases = earthMolarMassOfAtmosphereGases)
	
	{
		// Pa = P0 * e^(-(average molar mass of gases in the atmosphere in Kg/mol * speed of gravitation in m/s^2 / (universal constant of perfect gases in J K^(-1) mol(-1) * absolute temperature in K)) * altitude in m)
		return  airPressure_alt0 * Math.Pow(Math.E, molarMassOfAtmosphereGases * gravity / (universalConstantOfPerfectGases * absoluteTemperature) * -1 * altitude);
	}
}

public class Ship : MassCalculations
{
	public string name;
	public Unit_Mass mass = new Unit_Mass();    //in Kg, deducted
	public Unit_Mass shipEmptyMass;
	public List<Reactor> reactors;
	public List<FuelContainer> fuelContainers;
	public List<LandingGear> landingGears;
	public List<Turret> turrets;

	public Ship(string Name, float ShipEmptyMass, List<Reactor> Reactors,
	List<FuelContainer> FuelContainers, List<LandingGear> LandingGears,
	List<Turret> Turrets)
	{
		this.name = Name;
		this.shipEmptyMass = new Unit_Mass(ShipEmptyMass);
		this.reactors = Reactors;
		this.fuelContainers = FuelContainers;
		this.landingGears = LandingGears;
		this.turrets = Turrets;

		this.CalculateTotalMass();
	}

	public float CalculateTotalMass()
	{
		this.mass.isReadOnly = false;

		this.mass.Mass = this.shipEmptyMass.Mass +
		this.reactors.Sum(reactor => reactor.mass.Mass) +
		this.fuelContainers.Sum(fuelContainer => fuelContainer.CalculateTotalMass()) +
		this.landingGears.Sum(landingGear => landingGear.mass.Mass) +
		this.turrets.Sum(turret => turret.CalculateTotalMass());

		this.mass.isReadOnly = true;
		return this.mass.Mass;
	}

	public bool IsMassConsistent()
	{
		bool isValid = true;
		float simulated_fast_landingVelocity = 50f;

		Unit_Mass shipMassWtLGs = new Unit_Mass(this.CalculateTotalMass() - this.landingGears.Sum(landingGear => landingGear.mass.Mass));
		Unit_Mass LGsSupportCapacities = new Unit_Mass(this.landingGears.Sum(landingGear => landingGear.supportCapacity.Mass));
		if (LGsSupportCapacities.Mass > shipMassWtLGs.Mass) isValid &= true;
		else
		{
			isValid &= false;
			Console.WriteLine("Landing gears ({0}Kg of support capacities) cannot support the ship mass ({1}Kg without the landing gears).\n{2}Kg of support capacity needed at least.", LGsSupportCapacities.Mass, shipMassWtLGs.Mass, shipMassWtLGs.Mass - LGsSupportCapacities.Mass);
		}

		//if ()

		return isValid;
	}
}

public class Reactor
{
	public string name;
	public Unit_Mass mass;      // in Kg
	public Unit_Mass liftCapacity;
	public float energyConsumption;     // L/h or W/h ...
	public List<FuelContainer> connectedFuelContainers;     // list of Tanks and/or Batteries, etc..
	public List<FuelType> allowedFuelType;      // defined by each FuelContainer

	public Reactor(string Name, float Mass, float LiftCapacity, float EnergyConsumption, List<FuelContainer> ConnectedFuelContainers)
	{
		this.name = Name;
		this.mass = new Unit_Mass(Mass);
		this.mass.isReadOnly = true;
		this.liftCapacity = new Unit_Mass(LiftCapacity);
		this.energyConsumption = EnergyConsumption;
		this.connectedFuelContainers = ConnectedFuelContainers;
		this.allowedFuelType = this.connectedFuelContainers.Select(fuelContainer => fuelContainer.fuelType).Distinct().ToList();

		this.IsMassConsistent();
	}

	public bool IsMassConsistent()
	{
		bool isValid = true;

		if (this.liftCapacity.Mass > this.mass.Mass) isValid &= true;
		else
		{
			isValid &= false;
			Console.WriteLine("This reactor ({0}Kg) is not able to stand in the air.\n{1}Kg of lift capacity needed at least.", this.mass.Mass, this.mass.Mass - this.liftCapacity.Mass);
		}

		return isValid;
	}
}

public class FuelType : IEquatable<FuelType>
{
	public string name;
	public Unit_Mass mass;      // Kg/L or 0f for electricity
	public string unitOfMeasurement = "unknow";
	public float? price;        // by liters or watts

	public FuelType(string Name, float Mass, float? Price = null)
	{
		this.name = Name;
		this.mass = new Unit_Mass(Mass);
		this.mass.isReadOnly = true;
		this.price = Price;
	}
	
	// from System.IEquatable interface, used as reference for Enumerable.Distinct()
	public bool Equals(FuelType other)
	{
		if (Object.ReferenceEquals(other, null)) return false;
		if (Object.ReferenceEquals(this, other)) return true;
		return this.name.Equals(other.name);
	}
	public override int GetHashCode()
	{
		int hashFuelTypeName = this.name == null ? 0 : this.name.GetHashCode();
		return hashFuelTypeName;
	}
}

public class FuelContainer : MassCalculations
{
	public string name;
	public Unit_Mass mass = new Unit_Mass();
	public Unit_Mass emptyMass;     // in Kg
	public float capacity;          // L/m^2 or W/h
	public FuelType fuelType;
	private float _content = 0f;
	public float Content
	{
		get { return _content; }
		set
		{
			if (value < 0) throw new InvalidOperationException("Fuel container must be filled at at least 0f.");
			if (value > this.capacity) throw new InvalidOperationException($"Fuel Container cannot be filled more than its limit ({this.capacity}).");
			else this._content = value;
		}
	}

	public FuelContainer(string Name, float EmptyMass, float Capacity, FuelType FuelTypE)
	{
		this.name = Name;
		this.emptyMass = new Unit_Mass(EmptyMass);
		this.capacity = Capacity;
		this.fuelType = FuelTypE;

		this.CalculateTotalMass();
	}

	public float CalculateTotalMass()
	{
		this.mass.isReadOnly = false;
		this.mass.Mass = this.emptyMass.Mass + this.fuelType.mass.Mass * this.Content;
		this.mass.isReadOnly = true;
		return this.mass.Mass;
	}
}

public class LandingGear
{
	public string name;
	public Unit_Mass mass;
	public Unit_Mass supportCapacity;
	public float energyConsumption;     // W/h
	public float timeToDeploy;

	public LandingGear(string Name, float Mass, float SupportCapacity, float EnergyConsumption, float TimeToDeploy)
	{
		this.name = Name;
		this.mass = new Unit_Mass(Mass);
		this.mass.isReadOnly = true;
		this.supportCapacity = new Unit_Mass(SupportCapacity);
		this.energyConsumption = EnergyConsumption;
		this.timeToDeploy = TimeToDeploy;
	}

	public LandingGear DeepCopy()
	{
		return new LandingGear(this.name, this.mass.Mass, this.supportCapacity.Mass, this.energyConsumption, this.timeToDeploy);
	}
}

public class Turret : MassCalculations
{
	public string name;
	public Unit_Mass mass = new Unit_Mass();        // in Kg, general + deducted
	public Unit_Mass turretEmptyMass;
	private List<Barrel> _barrels;
	public List<Barrel> Barrels
	{
		get { return this._barrels; }
		init
		{
			if (value.Count() < 1) throw new InvalidOperationException("The number of barrels of a turret must be greater than 1.");
			else this._barrels = value;
		}
	}
	
	public float timeToChamber;
	public float shotsPerSec;
	public bool isManual;
	public bool isRemote;
	public bool isAuto;

	public int nbMags;
	public List<Magazine> magazines;
	public float timeToReload;      // in s


	public Turret(string Name, float TurretEmptyMass, List<Barrel> BarrelS,
	float ShotsPerSec, bool IsManual, bool IsRemote, bool IsAuto,
	int NbMags, Magazine MagazineType, float TimeToReload)
	{
		this.name = Name;
		this.turretEmptyMass = new Unit_Mass(TurretEmptyMass);
		this.Barrels = BarrelS;
		this.shotsPerSec = ShotsPerSec;
		this.isManual = IsManual;
		this.isRemote = IsRemote;
		this.isAuto = IsAuto;
		this.nbMags = NbMags;
		// then fill the magazine list thanks to nbMags
		this.magazines = new List<Magazine>(Enumerable.Range(0, this.nbMags).Select(magazine => MagazineType.DeepCopy()));

		this.timeToReload = TimeToReload;
		this.CalculateTotalMass();
	}

	// can we use a get and setter instead to track updated magazine mass ?
	public float CalculateTotalMass()
	{
		this.mass.isReadOnly = false;
		this.mass.Mass = this.turretEmptyMass.Mass + this.Barrels.Sum(barrel => barrel.mass.Mass) + this.magazines.Sum(magazine => magazine.CalculateTotalMass());
		this.mass.isReadOnly = true;
		return this.mass.Mass;
	}
	public void InitMags()
	{

	}
	public void EjectMag()
	{
		// remove the magazine from the list of magazines
		// update the turret mass
	}
}

public class Barrel
{
	public string name;
	public Unit_Mass mass;
	public List<AmmoType> ammoType { get; init; }
	public double resilience;	// J joul
	
	public Barrel(string Name, float Mass, List<AmmoType> AmmoTypE, double Resilience)
	{
		this.name = Name;
		this.mass = new Unit_Mass(Mass);
		this.ammoType = AmmoTypE;
		this.resilience = Resilience;
	}
	
	public Barrel DeepCopy()
	{
		return new Barrel(this.name, this.mass.Mass, this.ammoType, this.resilience);
	}
}

public class Magazine : MassCalculations
{
	public string name;
	public Unit_Mass mass = new Unit_Mass();
	public Unit_Mass magEmptyMass;      // in Kg
	private int _ammoCapacity;
	public int AmmoCapacity
	{
		get { return this._ammoCapacity; }
		init
		{
			if (value > 0) this._ammoCapacity = value;
			else throw new InvalidOperationException("The ammunition capacity of a magazine must be greater than 0.");
		}
	}
	private int _ammo = 0;
	public int Ammo
	{
		get { return _ammo; }
		set
		{
			this._ammo = value;
			this.CalculateTotalMass();
			if (this._ammo <= 0) {/* remove this magazine from the list */}
			if (value > this.AmmoCapacity) throw new InvalidOperationException($"The number of ammunition cannot be greater than the magazine's capacity ({this.AmmoCapacity}).");
		}
	}
	public AmmoType ammoType { get; init; }

	public Magazine(string Name, float MagEmptyMasS, int AmmoCapacitY, AmmoType AmmoTypE, int AmmO = 0)
	{
		this.name = Name;
		this.magEmptyMass = new Unit_Mass(MagEmptyMasS);
		this.AmmoCapacity = AmmoCapacitY;
		this.ammoType = AmmoTypE;
		this.Ammo = AmmO;
		this.CalculateTotalMass();
	}

	public float CalculateTotalMass()
	{
		this.mass.isReadOnly = false;
		this.mass.Mass = this.magEmptyMass.Mass + this.Ammo * this.ammoType.mass.Mass;
		this.mass.isReadOnly = true;
		return this.mass.Mass;
	}

	public Magazine DeepCopy()
	{
		return new Magazine(this.name, this.magEmptyMass.Mass, this.AmmoCapacity, this.ammoType, this.Ammo);
	}
}

public class AmmoType
{
	public readonly string name;
	public readonly Unit_Mass mass;     // in Kg
	public readonly float speed;        // in M/s
	public readonly float damage;
	public float? price = null;     // per unit

	public AmmoType(string Name, float Mass, float Speed, float Damage, float? Price = null)
	{
		this.name = Name;
		this.mass = new Unit_Mass(Mass);
		this.speed = Speed;
		this.damage = Damage;
		this.price = Price;
	}
}
