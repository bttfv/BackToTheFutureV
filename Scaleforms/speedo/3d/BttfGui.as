class BttfGui extends MovieClip
{
	var globalMC : MovieClip
	
	var digit1MC : MovieClip
	var digit2MC : MovieClip
	var digit3MC : MovieClip
	
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
		this.digit3MC = this.globalMC.attachMovie("digit", "digit3", this.globalMC.getNextHighestDepth());
		
		this.digit1MC._x = 201;
		this.digit1MC._y = 42;
		this.digit2MC._x = 394;
		this.digit2MC._y = 42;		
		this.digit3MC._x = 582;
		this.digit3MC._y = 42;
		
		this.digit1MC.gotoAndStop(11);
		this.digit2MC.gotoAndStop(1);
		this.digit3MC.gotoAndStop(1);
	}
	
	// It's a common thing to name publicly available methods this way
	function SET_DIGIT_1(digit)
	{
		this.digit1MC.gotoAndStop(digit + 1);
	}
	
	function SET_DIGIT_2(digit)
	{
		this.digit2MC.gotoAndStop(digit + 1);
	}
	
	function SET_DIGIT_3(digit)
	{
		this.digit3MC.gotoAndStop(digit + 1);
	}
}
