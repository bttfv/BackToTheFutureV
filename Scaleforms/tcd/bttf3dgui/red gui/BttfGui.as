import flash.display.*;

class BttfGui extends MovieClip
{
	var globalMC : MovieClip
	
	var redSlot : BttfGuiSlot
	
	// Constructor
	function BttfGui(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		this.redSlot = new BttfGuiSlot("red", this.globalMC, 0);
			
	}
	
	function SET_MONTH(month)
	{
		redSlot.setMonth(month);
	}
	
	function SET_DAY(day)
	{
		redSlot.setDay(day);
	}
	
	function SET_YEAR(year)
	{
		redSlot.setYear(year);
	}
	
	function SET_HOUR(hour)
	{
		redSlot.setHour(hour);
	}
	
	function SET_MINUTE(minute)
	{
		redSlot.setMinute(minute);
	}
}
