class BttfGui extends MovieClip
{
	var globalMC : MovieClip
	
	var normalFlux : MovieClip
	var blueFlux : MovieClip
	var orangeFlux : MovieClip
	
	// Constructor
	function BttfGui(globalMovieClip)
	{
		// Invoke parent class constructor
		super();
		
		// Enable gfx extensions
		_global.gfxExtensions = true;
		
		// Save ref to global movie clip
		this.globalMC = globalMovieClip;
		
		this.normalFlux = this.globalMC.attachMovie("bttf_flux", "bttf_flux", this.globalMC.getNextHighestDepth());
		this.normalFlux.stop();
		
		this.blueFlux = this.globalMC.attachMovie("flux_blue", "flux_blue", this.globalMC.getNextHighestDepth());
		this.blueFlux.stop();
		this.blueFlux._visible = false;
		
		this.orangeFlux = this.globalMC.attachMovie("flux_orange", "flux_orange", this.globalMC.getNextHighestDepth());
		this.orangeFlux.stop();
		this.orangeFlux._visible = false;
	}
	
	function START_BLUE_ANIMATION()
	{
		this.normalFlux.gotoAndStop(1);
		this.normalFlux._visible = false;
		
		this.blueFlux._visible = true;
	}
	
	function STOP_BLUE_ANIMATION()
	{
		this.blueFlux.stop();
	}
	
	function START_ORANGE_ANIMATION()
	{
		this.normalFlux.gotoAndStop(1);
		this.normalFlux._visible = false;
		
		this.orangeFlux._visible = true;
	}
	
	function STOP_ORANGE_ANIMATION()
	{
		this.orangeFlux.stop();
	}
	
	function START_ANIMATION()
	{		
		this.normalFlux.gotoAndPlay(1);
		this.normalFlux._visible = true;

		this.blueFlux._visible = false;
		this.orangeFlux._visible = false;
	}
	
	function STOP_ANIMATION()
	{
		this.normalFlux.stop();
	}
}
