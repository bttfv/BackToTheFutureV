import flash.display.*;

class BttfGui extends MovieClip
{
	var globalMC : MovieClip
	
	var yellowSlot : BttfGuiSlot
	
	// Constructor
	function BttfGui(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		this.yellowSlot = new BttfGuiSlot("yellow", this.globalMC, 0);
			
	}
	
	function SET_MONTH(month)
	{
		yellowSlot.setMonth(month);
	}
	
	function SET_DAY(day)
	{
		yellowSlot.setDay(day);
	}
	
	function SET_YEAR(year)
	{
		yellowSlot.setYear(year);
	}
	
	function SET_HOUR(hour)
	{
		yellowSlot.setHour(hour);
	}
	
	function SET_MINUTE(minute)
	{
		yellowSlot.setMinute(minute);
	}
}
