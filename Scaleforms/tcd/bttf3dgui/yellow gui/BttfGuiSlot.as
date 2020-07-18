class BttfGuiSlot
{
	var monthMC : MovieClip
	
	var day1 : MovieClip
	var day2 : MovieClip
	
	var year1 : MovieClip
	var year2 : MovieClip
	var year3 : MovieClip
	var year4 : MovieClip
	
	var hour1 : MovieClip
	var hour2 : MovieClip
	
	var minute1 : MovieClip
	var minute2 : MovieClip
	
	var globalMC : MovieClip
	var typeSlot : String
	
	function BttfGuiSlot(typeSlot, global, yOffset)
	{
		this.globalMC = global;
		this.typeSlot = typeSlot;
		
		// Initialize the month MovieClip and setup the position
		this.monthMC = this.globalMC.attachMovie(typeSlot + "Month", typeSlot + "Month", this.globalMC.getNextHighestDepth());
		this.monthMC.stop();
		this.monthMC._x = 62;
		this.monthMC._y = 88 + yOffset;
		
		// Initialize the 2 day MovieClips and setup their position
		this.day1 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Day1", this.globalMC.getNextHighestDepth());
		this.day1.stop();
		this.day1._x = 370;
		this.day1._y = 92 + yOffset;
		
		this.day2 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Day2", this.globalMC.getNextHighestDepth());
		this.day2.stop();
		this.day2._x = 370 + 85;
		this.day2._y = 92 + yOffset;
		
		
		// Initialize the 4 year MovieClips and setup their positions
		this.year1 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Year1", this.globalMC.getNextHighestDepth());
		this.year1.stop()
		this.year1._x = 615;
		this.year1._y = 92 + yOffset;
		
		this.year2 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Year2", this.globalMC.getNextHighestDepth());
		this.year2.stop();
		this.year2._x = 615 + 85;
		this.year2._y = 92 + yOffset;
		
		this.year3 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Year3", this.globalMC.getNextHighestDepth());
		this.year3.stop();
		this.year3._x = 615 + 85 + 85;
		this.year3._y = 92 + yOffset;
		
		this.year4 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Year4", this.globalMC.getNextHighestDepth());
		this.year4.stop();
		this.year4._x = 615 + 85 + 85 + 85;
		this.year4._y = 92 + yOffset;

		
		// Initialize the 2 hour MovieClips and setup their position
		this.hour1 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Hour1", this.globalMC.getNextHighestDepth());
		this.hour1.stop();
		this.hour1._x = 1070;
		this.hour1._y = 92 + yOffset;
		
		
		this.hour2 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Hour2", this.globalMC.getNextHighestDepth());
		this.hour2.stop();
		this.hour2._x = 1070 + 85;
		this.hour2._y = 92 + yOffset;

		// Initialize the 2 minute MovieClips and setup their position
		this.minute1 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Minute1", this.globalMC.getNextHighestDepth());
		this.minute1.stop();
		this.minute1._x = 1305;
		this.minute1._y = 92 + yOffset;
		
		this.minute2 = this.globalMC.attachMovie(typeSlot + "Digit", typeSlot + "Minute2", this.globalMC.getNextHighestDepth());
		this.minute2.stop();
		this.minute2._x = 1305 + 85;
		this.minute2._y = 92 + yOffset;
	}
	
	function setMonth(month)
	{
		if(month == -1)
		{
			this.monthMC.gotoAndStop(25);
			return;
		}
		
		this.monthMC.gotoAndStop(month);
	}
	
	function setDay(day)
	{
		if(day == -1)
		{
			this.day1.gotoAndStop(25);
			this.day2.gotoAndStop(25);
			
			return;
		}
		
		var output = [];
		var sNumber = day.toString();
	
		for (var i = 0, len = sNumber.length; i < len; i += 1) 
		{
			output.push(parseInt(sNumber.charAt(i)));
		}
		
		if(output.length > 2)
		{
			return;
		}
		
		if(output.length == 1)
		{
			this.day1.gotoAndStop(1);
			this.day2.gotoAndStop(output[0] + 1);
		}
		else
		{
			this.day1.gotoAndStop(output[0] + 1);
			this.day2.gotoAndStop(output[1] + 1);
		}
	}
	
	function setYear(year)
	{
		if(year == -1)
		{
			this.year1.gotoAndStop(25);
			this.year2.gotoAndStop(25);
			this.year3.gotoAndStop(25);
			this.year4.gotoAndStop(25);
			
			return;
		}
		
		var output = [];
		var sNumber = year.toString();
	
		for (var i = 0, len = sNumber.length; i < len; i += 1) 
		{
			output.push(parseInt(sNumber.charAt(i)));
		}
		
		if(output.length > 4)
		{
			return;
		}
		
		if(output.length == 1)
		{
			this.year1.gotoAndStop(1);
			this.year2.gotoAndStop(1);
			this.year3.gotoAndStop(1);
			this.year4.gotoAndStop(output[0] + 1);
		}
		else if(output.length == 2)
		{
			this.year1.gotoAndStop(1);
			this.year2.gotoAndStop(1);
			this.year3.gotoAndStop(output[0] + 1);
			this.year4.gotoAndStop(output[1] + 1);
		}
		else if(output.length == 3)
		{
			this.year1.gotoAndStop(1);
			this.year2.gotoAndStop(output[0] + 1);
			this.year3.gotoAndStop(output[1] + 1);
			this.year4.gotoAndStop(output[2] + 1);
		}
		else
		{
			this.year1.gotoAndStop(output[0] + 1);
			this.year2.gotoAndStop(output[1] + 1);
			this.year3.gotoAndStop(output[2] + 1);
			this.year4.gotoAndStop(output[3] + 1);
		}
	}
	
	function setHour(hour)
	{
		if(hour == -1)
		{
			this.hour1.gotoAndStop(25);
			this.hour2.gotoAndStop(25);
			
			return;
		}
		
		var output = [];
		var sNumber = hour.toString();
	
		for (var i = 0, len = sNumber.length; i < len; i += 1) 
		{
			output.push(parseInt(sNumber.charAt(i)));
		}
		
		if(output.length > 2)
		{
			return;
		}
		
		if(output.length == 1)
		{
			this.hour1.gotoAndStop(1);
			this.hour2.gotoAndStop(output[0] + 1);
		}
		else
		{
			this.hour1.gotoAndStop(output[0] + 1);
			this.hour2.gotoAndStop(output[1] + 1);
		}
	}
	
	function setMinute(minute)
	{
		if(minute == -1)
		{
			this.minute1.gotoAndStop(25);
			this.minute2.gotoAndStop(25);
			
			return;
		}
		
		var output = [];
		var sNumber = minute.toString();
	
		for (var i = 0, len = sNumber.length; i < len; i += 1) 
		{
			output.push(parseInt(sNumber.charAt(i)));
		}
		
		if(output.length > 2)
		{
			return;
		}
		
		if(output.length == 1)
		{
			this.minute1.gotoAndStop(1);
			this.minute2.gotoAndStop(output[0] + 1);
		}
		else
		{
			this.minute1.gotoAndStop(output[0] + 1);
			this.minute2.gotoAndStop(output[1] + 1);
		}
	}
}