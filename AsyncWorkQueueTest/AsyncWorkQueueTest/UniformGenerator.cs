using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncWorkQueueTest
{
    public interface Generator
    {
        double nextValue(int low, int high);
    }
    public class UniformGenerator: Generator
    {
        const int H = 32768; 
	    const int Maxgen = 100; 
	    const int InitialSeed = 0;
	    const int LastSeed = 1;
	    const int NewSeed = 2;
	    const int SeedType = 3;
	    long [] aw = new long[4];
	    long [] avw = new long[4];  /* a[j]^{2^w} et a[j]^{2^{v+w}}. */
	    long [] a = { 45991, 207707, 138556, 49689};
	    long [] m = { 2147483647, 2147483543, 2147483423, 2147483323};

	    long [,] Ig = new long[4,Maxgen+1];
	    long [,] Lg = new long[4,Maxgen+1];
	    long [,] Cg = new long[4,Maxgen+1];
        /* Initial seed, previous seed, and current seed. */

	    static short i, j;
        public UniformGenerator()
        {
            this.InitDefault();
        }
	    private long multModM( long s, long t, long M) {
		    /* Returns (s*t) MOD M. Assumes that -M < s < M and -M < t < M. */
		    /* See L'Ecuyer and Cote (1991). */
		    long R, S0, S1, q, qh, rh, k;
			
		    if( s<0) s+=M;
		    if( t<0) t+=M;
		    if( s<H) { S0=s; R=0;}
		    else {
		        S1=s/H; S0=s-H*S1;
		        qh=M/H; rh=M-H*qh;
		        if( S1>=H) {
		        S1-=H; k=t/qh; R=H*(t-k*qh)-k*rh;
		        while( R<0) R+=M;
		    }
		    else R=0;
		    if( S1!=0) {
		        q=M/S1; k=t/q; R-=k*(M-S1*q);
		        if( R>0) R-=M;
		        R += S1*(t-k*q);
		        while( R<0) R+=M;
		    }
		    k=R/qh; R=H*(R-k*qh)-k*rh;
		    while( R<0) R+=M;
		    }
		    if( S0!=0) {
		        q=M/S0; k=t/q; R-=k*(M-S0*q);
		        if( R>0) R-=M;
		        R+=(S0*(t-k*q));
		        while( R<0) R+=M;
		    }
		    return R;
	    }
		
		
	    /*---------------------------------------------------------------------*/
	    /* Public part. */
	    /*---------------------------------------------------------------------*/
	    public void setSeed(int g, long [] s) {
		    if( g>Maxgen) Console.WriteLine( "ERROR: SetSeed with g > Maxgen\n");
		    for( j=0; j<4; j++) Ig[j,g]=s[j];
		    initGenerator( g, InitialSeed);
	    }
		
		
	    public void writeState( int g) {
		    Console.WriteLine("State of generator g = "+g);
		    for( j=0; j<4; j++) Console.WriteLine("Cg["+j+"] = "+Cg[j,g]);
	    }
		
		
	    public void getState(int g, long [] s) {
		    for( j=0; j<4; j++) s[j]=Cg[j,g];
	    }
		
		
	    public void initGenerator(int g, int where) {
		    if( g>Maxgen) Console.WriteLine( "ERROR: InitGenerator with g > Maxgen\n");
		    for( j=0; j<4; j++) {
			    switch (where) {
				    case InitialSeed :
				    Lg[j,g]=Ig[j,g]; break;
				    case NewSeed :
				    Lg[j,g]=multModM( aw[j], Lg[j,g], m[j]); break;
				    case LastSeed :
				    break;
			    }
			    Cg[j,g]=Lg[j,g];
		    }
	    }
		
		
	    public void setInitialSeed( long [] s) {
		    int g;
		
		    for( j=0; j<4; j++) Ig[j,0]=s[j];
		    initGenerator( 0, InitialSeed);
		    for( g=1; g<=Maxgen; g++) {
			    for( j=0; j<4; j++) Ig[j,g]=multModM( avw[j], Ig[j,g-1], m[j]);
			    initGenerator( g, InitialSeed);
		    }
	    }
		
		
	    public void init( long v, long w) {
		    long [] sd = {11111111, 22222222, 33333333, 44444444};
		
		    for( j=0; j<4; j++) {
		    for( aw[j]=a[j], i=1; i<=w; i++) aw[j]=multModM( aw[j], aw[j], m[j]);
			    for( avw[j]=aw[j], i=1; i<=v; i++) avw[j]=multModM( avw[j], avw[j], m[j]);
		    }
		    setInitialSeed (sd);
	    }

        public double nextValue(int low, int high)
        {
            double nextUniform = nextValue(1);
            return (high - low) * nextUniform + low;
        }
	    public double nextValue(int g) {
		    long k,s;
		    double u=0.0;
		
		    if( g>Maxgen) Console.WriteLine( "ERROR: Genval with g > Maxgen\n");
		
		    s=Cg[0,g]; k=s/46693;
		    s=45991*(s-k*46693)-k*25884;
		    if( s<0) s+=2147483647; 
		    Cg[0,g]=s;
		    u+=(4.65661287524579692e-10*s);
		
		    s=Cg[1,g]; k=s/10339;
		    s=207707*(s-k*10339)-k*870;
		    if( s<0) s+=2147483543;  
		    Cg[1,g]=s;
		    u-=(4.65661310075985993e-10*s);
		    if( u<0) u+=1.0;
		
		    s=Cg[2,g]; k=s/15499;
		    s=138556*(s-k*15499)-k*3979;
		    if( s<0.0) s+=2147483423;  
		    Cg[2,g]=s;
		    u+=(4.65661336096842131e-10*s);
		    if( u>=1.0) u-=1.0;
		
		    s=Cg[3,g]; k=s/43218;
		    s=49689*(s-k*43218)-k*24121;
		    if( s<0) s+=2147483323;  
		    Cg[3,g]=s;
		    u-=(4.65661357780891134e-10*s);
		    if( u<0) u+=1.0;
		
		    return (u);
	    }

	    public void InitDefault() {
		    init( 31, 41);
	    }

	    public static void main(String[] args) {
		    long [] s = new long[4];
		  
		    UniformGenerator generator = new UniformGenerator();
		    generator.InitDefault();
		    for( i=1; i<=10; i++) Console.WriteLine( i+": "+generator.nextValue(1));
		    Console.WriteLine( "10 U(0,1) random numbers from generator 2:");
		    for( i=1; i<=10; i++) Console.WriteLine( i+": "+generator.nextValue(2));
		    generator.getState( 1,s);
		    Console.WriteLine("\nThe 11th U(0,1) random number (*) from generator 1 is: "+
		              generator.nextValue(1));
		    Console.WriteLine("Let's \"bookmark\" the state here and then get another\n");
		    Console.WriteLine( "10 numbers from generator 1:\n");
		    Console.WriteLine( "Let's \"bookmark\" the state here and then get another\n");
		    Console.WriteLine( "10 numbers from generator 1:\n");
		    for( i=1; i<=10; i++) Console.WriteLine( i+": "+ generator.nextValue(1));
		    generator.setSeed( 1,s);
		    Console.WriteLine( "\nAfter resetting the seed to the above \"bookmark\",\n");
		    Console.WriteLine( "we should get back the 11th number (*) above: ");
		    Console.WriteLine( generator.nextValue(1));

		      /* test InitGenerator ... */
		    Console.WriteLine( "\n10 U(0,1) from generator 3:\n");
		    for( i=1; i<=10; i++) Console.WriteLine( i+": "+generator.nextValue(3));
		    Console.WriteLine( "moving back to the last seed of generator 3 by\n");
		    Console.WriteLine( "  InitGenerator(3,LastSeed);\n");
		    generator.initGenerator(3,LastSeed);
		    Console.WriteLine( "We should get back the same 10 numbers:\n");
		    for( i=1; i<=10; i++) Console.WriteLine( i+": "+generator.nextValue(3));
	    }

    }
}
