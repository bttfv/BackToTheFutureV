class BttfGui extends MovieClip
{
	var globalMC : MovieClip

	var wormholeMC : MovieClip;

	// Constructor
	function BttfGui(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		// Init main movie clip
    	this.wormholeMC = this.globalMC.attachMovie("wormhole", "wormhole", this.globalMC.getNextHighestDepth());
		this.wormholeMC.stop();
			
		// Will not be visible anywhere in game itself!
		// But will be visible inside Flash Pro when running
		trace("Hello from Boilerplate class!");
	}
	
	function SET_WORMHOLE_FRAME(frame)
	{
		this.wormholeMC.gotoAndStop(frame);
	}
	
	function START_ANIMATION()
	{		
		this.wormholeMC.gotoAndPlay(1);
	}
	
	function START_ANIMATION_BTTF2()
	{
		this.wormholeMC.gotoAndPlay(77);
	}

}
