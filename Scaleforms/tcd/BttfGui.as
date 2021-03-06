import flash.display.*;

class BttfGui extends MovieClip
{
	var globalMC : MovieClip
	
	var backgroundMC : MovieClip
	
	var redSlot : BttfGuiSlot
	var greenSlot : BttfGuiSlot
	var yellowSlot : BttfGuiSlot
	
	var speedoGui : BttfSpeedoGui
	
	var empty : MovieClip
	
	var ticking : MovieClip
	
	// Constructor
	function BttfGui(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		this.backgroundMC = this.globalMC.attachMovie("tcdBackground", "tcdBackground", this.globalMC.getNextHighestDepth());
		this.backgroundMC.stop();
		this.backgroundMC._x = 0;
		this.backgroundMC._y = 222;
		
		this.redSlot = new BttfGuiSlot("red", this.globalMC, 0);
		this.greenSlot = new BttfGuiSlot("green", this.globalMC, 291);
		this.yellowSlot = new BttfGuiSlot("yellow", this.globalMC,  585);
				
		this.empty = this.globalMC.attachMovie("empty", "empty", this.globalMC.getNextHighestDepth());
		this.empty.stop();
		this.empty._x = 1309;
		this.empty._y = 98;
		
		this.speedoGui = new BttfSpeedoGui(this.globalMC);
		
		this.ticking = this.globalMC.attachMovie("ticking", "ticking", this.globalMC.getNextHighestDepth());
		this.ticking.stop();
		this.ticking._x = 0;
		this.ticking._y = 222;
		
	}
	
	function SET_TCD_BACKGROUND(bgName)
	{
		if(bgName == "metal")
		{
			backgroundMC.gotoAndStop(1);
		}
		else if(bgName == "trans")
		{
			backgroundMC.gotoAndStop(2);
		}
	}
	
	function SET_DIODE_STATE(state)
	{
		if(state)
		{
			ticking.gotoAndStop(2);
		}
		else
		{
			ticking.gotoAndStop(1);
		}
	}
	
	function SET_RED_MONTH(month)
	{
		redSlot.setMonth(month);
	}
	
	function SET_RED_DAY(day)
	{
		redSlot.setDay(day);
	}
	
	function SET_RED_YEAR(year)
	{
		redSlot.setYear(year);
	}
	
	function SET_RED_HOUR(hour)
	{
		redSlot.setHour(hour);
	}
	
	function SET_RED_MINUTE(minute)
	{
		redSlot.setMinute(minute);
	}
	
	function SET_GREEN_MONTH(month)
	{
		greenSlot.setMonth(month);
	}
	
	function SET_GREEN_DAY(day)
	{
		greenSlot.setDay(day);
	}
	
	function SET_GREEN_YEAR(year)
	{
		greenSlot.setYear(year);
	}
	
	function SET_GREEN_HOUR(hour)
	{
		greenSlot.setHour(hour);
	}
	
	function SET_GREEN_MINUTE(minute)
	{
		greenSlot.setMinute(minute);
	}
	
	function SET_YELLOW_MONTH(month)
	{
		yellowSlot.setMonth(month);
	}
	
	function SET_YELLOW_DAY(day)
	{
		yellowSlot.setDay(day);
	}
	
	function SET_YELLOW_YEAR(year)
	{
		yellowSlot.setYear(year);
	}
	
	function SET_YELLOW_HOUR(hour)
	{
		yellowSlot.setHour(hour);
	}
	
	function SET_YELLOW_MINUTE(minute)
	{
		yellowSlot.setMinute(minute);
	}
	
	function HIDE_EMPTY()
	{
		empty.gotoAndStop(3);
	}
	
	function SET_EMPTY_STATE(state)
	{
		if(state)
		{
			empty.gotoAndStop(1);
		}
		else
		{
			empty.gotoAndStop(2);
		}
	}
	
	function SET_SPEEDO_BACKGROUND(_state) 
	{
		speedoGui.setBackground(_state);
	}
	
	function SET_DIGIT_1(digit)
	{
		speedoGui.setDigit1(digit);
	}
	
	function SET_DIGIT_2(digit)
	{
		speedoGui.setDigit2(digit);
	}
	
	function SET_DIGIT_3(digit)
	{
		speedoGui.setDigit3(digit);
	}
	
	function SET_AM_PM(type, amPmToggle)
	{
		switch(type)
		{
			case "red":
			redSlot.setAmPm(amPmToggle);
			break;
			
			case "yellow":
			yellowSlot.setAmPm(amPmToggle);
			break;
			
			case "green":
			greenSlot.setAmPm(amPmToggle);
			break;
		}
	}
}
