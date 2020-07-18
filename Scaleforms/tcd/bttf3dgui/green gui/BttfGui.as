import flash.display.*;

class BttfGui extends MovieClip
{
	var globalMC : MovieClip
	
	var greenSlot : BttfGuiSlot
	
	// Constructor
	function BttfGui(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		this.greenSlot = new BttfGuiSlot("green", this.globalMC, 0);
	}
	
	function SET_MONTH(month)
	{
		greenSlot.setMonth(month);
	}
	
	function SET_DAY(day)
	{
		greenSlot.setDay(day);
	}
	
	function SET_YEAR(year)
	{
		greenSlot.setYear(year);
	}
	
	function SET_HOUR(hour)
	{
		greenSlot.setHour(hour);
	}
	
	function SET_MINUTE(minute)
	{
		greenSlot.setMinute(minute);
	}
}
