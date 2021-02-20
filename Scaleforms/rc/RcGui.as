class RcGui extends MovieClip
{
	var globalMC : MovieClip

	var digit1 : MovieClip
	var digit2 : MovieClip
	var digit3 : MovieClip
	
	var stopLed : MovieClip
	
	// Constructor
	function RcGui(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		digit1 = this.globalMC.attachMovie("speedoDigit", "speedoDigit1", this.globalMC.getNextHighestDepth());
		digit1._x = 410;
		digit1._y = 165;
		digit1.gotoAndStop(11);
		
		digit2 = this.globalMC.attachMovie("speedoDigit", "speedoDigit2", this.globalMC.getNextHighestDepth());
		digit2._x = 600;
		digit2._y = 165;
		digit2.gotoAndStop(1);
		
		digit3 = this.globalMC.attachMovie("speedoDigit", "speedoDigit3", this.globalMC.getNextHighestDepth());
		digit3._x = 795;
		digit3._y = 165;
		digit3.gotoAndStop(1);
		
		stopLed = this.globalMC.attachMovie("stopLed", "STOP_LED", this.globalMC.getNextHighestDepth());
		stopLed._x = 92.5;
		stopLed._y = 357;
		stopLed.gotoAndStop(1);
	}

	function SET_DIGIT_1(digit) 
	{
		digit1.gotoAndStop(digit + 1);
	}
	
	function SET_DIGIT_2(digit) 
	{
		digit2.gotoAndStop(digit + 1);
	}
	
	function SET_DIGIT_3(digit) 
	{
		digit3.gotoAndStop(digit + 1);
	}
	
	function SET_STOP(_state) 
	{		
		stopLed.gotoAndStop(_state + 1);
	}
}