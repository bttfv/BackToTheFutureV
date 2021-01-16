class BttfSpeedoGui
{	
	var digit1MC : MovieClip
	var digit2MC : MovieClip
	
	// Constructor
	function BttfSpeedoGui(globalMovieClip)
	{		
		this.digit1MC = globalMovieClip.attachMovie("digit", "digit1", globalMovieClip.getNextHighestDepth());
		this.digit2MC = globalMovieClip.attachMovie("digit", "digit2", globalMovieClip.getNextHighestDepth());
		
		this.digit1MC._x = 201;
		this.digit1MC._y = 115;
		this.digit2MC._x = 339;
		this.digit2MC._y = 115;
		
		this.digit1MC.gotoAndStop(15);
		this.digit2MC.stop();
		
	}
	
	// It's a common thing to name publicly available methods this way
	function setDigit1(digit)
	{
		if(digit == -1)
		{
			this.digit1MC.gotoAndStop(15);
			return;
		}
		
		this.digit1MC.gotoAndStop(digit + 1);
	}
	
	function setDigit2(digit)
	{
		if(digit == -1)
		{
			this.digit2MC.gotoAndStop(15);
			return;
		}
		
		this.digit2MC.gotoAndStop(digit + 1);
	}
}
