class BttfGui extends MovieClip
{
	var globalMC : MovieClip
	
	var digit1MC : MovieClip
	var digit2MC : MovieClip
	
	// Constructor
	function BttfGui(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		this.digit1MC = this.globalMC.attachMovie("digit", "digit1", this.globalMC.getNextHighestDepth());
		this.digit2MC = this.globalMC.attachMovie("digit", "digit2", this.globalMC.getNextHighestDepth());
		
		this.digit1MC._x = 201;
		this.digit1MC._y = 42;
		this.digit2MC._x = 394;
		this.digit2MC._y = 42;
		
		this.digit1MC.stop();
		this.digit2MC.stop();
		
		// Will not be visible anywhere in game itself!
		// But will be visible inside Flash Pro when running
		trace("Hello from Boilerplate class!");
	}
	
	// It's a common thing to name publicly available methods this way
	function SET_DIGIT_1(digit)
	{
		if(digit == -1)
		{
			this.digit1MC.gotoAndStop(15);
			return;
		}
		
		this.digit1MC.gotoAndStop(digit + 1);
	}
	
	function SET_DIGIT_2(digit)
	{
		if(digit == -1)
		{
			this.digit2MC.gotoAndStop(15);
			return;
		}
		
		this.digit2MC.gotoAndStop(digit + 1);
	}
}
