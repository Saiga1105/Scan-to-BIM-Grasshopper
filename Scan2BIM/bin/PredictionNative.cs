/*
* MATLAB Compiler: 6.2 (R2016a)
* Date: Wed Jan 25 17:04:45 2017
* Arguments: "-B" "macro_default" "-W" "dotnet:Classification,Prediction,0.0,private"
* "-T" "link:lib" "-d" "D:\Google Drive\Research\2016-11 Build Grasshopper plug in\Matlab
* library\Classification\for_testing" "-v" "class{Prediction:D:\Google
* Drive\Research\2016-11 Build Grasshopper plug in\Matlab library\predictfunction.m}"
* "-a" "D:\Google Drive\Research\2016-11 Build Grasshopper plug in\Matlab
* library\model.mat" 
*/
using System;
using System.Reflection;
using System.IO;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;

#if SHARED
[assembly: System.Reflection.AssemblyKeyFile(@"")]
#endif

namespace ClassificationNative
{

  /// <summary>
  /// The Prediction class provides a CLS compliant, Object (native) interface to the
  /// MATLAB functions contained in the files:
  /// <newpara></newpara>
  /// D:\Google Drive\Research\2016-11 Build Grasshopper plug in\Matlab
  /// library\predictfunction.m
  /// </summary>
  /// <remarks>
  /// @Version 0.0
  /// </remarks>
  public class Prediction : IDisposable
  {
    #region Constructors

    /// <summary internal= "true">
    /// The static constructor instantiates and initializes the MATLAB Runtime instance.
    /// </summary>
    static Prediction()
    {
      if (MWMCR.MCRAppInitialized)
      {
        try
        {
          Assembly assembly= Assembly.GetExecutingAssembly();

          string ctfFilePath= assembly.Location;

          int lastDelimiter= ctfFilePath.LastIndexOf(@"\");

          ctfFilePath= ctfFilePath.Remove(lastDelimiter, (ctfFilePath.Length - lastDelimiter));

          string ctfFileName = "Classification.ctf";

          Stream embeddedCtfStream = null;

          String[] resourceStrings = assembly.GetManifestResourceNames();

          foreach (String name in resourceStrings)
          {
            if (name.Contains(ctfFileName))
            {
              embeddedCtfStream = assembly.GetManifestResourceStream(name);
              break;
            }
          }
          mcr= new MWMCR("",
                         ctfFilePath, embeddedCtfStream, true);
        }
        catch(Exception ex)
        {
          ex_ = new Exception("MWArray assembly failed to be initialized", ex);
        }
      }
      else
      {
        ex_ = new ApplicationException("MWArray assembly could not be initialized");
      }
    }


    /// <summary>
    /// Constructs a new instance of the Prediction class.
    /// </summary>
    public Prediction()
    {
      if(ex_ != null)
      {
        throw ex_;
      }
    }


    #endregion Constructors

    #region Finalize

    /// <summary internal= "true">
    /// Class destructor called by the CLR garbage collector.
    /// </summary>
    ~Prediction()
    {
      Dispose(false);
    }


    /// <summary>
    /// Frees the native resources associated with this object
    /// </summary>
    public void Dispose()
    {
      Dispose(true);

      GC.SuppressFinalize(this);
    }


    /// <summary internal= "true">
    /// Internal dispose function
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (!disposed)
      {
        disposed= true;

        if (disposing)
        {
          // Free managed resources;
        }

        // Free native resources
      }
    }


    #endregion Finalize

    #region Methods

    /// <summary>
    /// Provides a single output, 0-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction()
    {
      return mcr.EvaluateFunction("predictfunction", new Object[]{});
    }


    /// <summary>
    /// Provides a single output, 1-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area)
    {
      return mcr.EvaluateFunction("predictfunction", Area);
    }


    /// <summary>
    /// Provides a single output, 2-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity);
    }


    /// <summary>
    /// Provides a single output, 3-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ);
    }


    /// <summary>
    /// Provides a single output, 4-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY);
    }


    /// <summary>
    /// Provides a single output, 5-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height);
    }


    /// <summary>
    /// Provides a single output, 6-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity);
    }


    /// <summary>
    /// Provides a single output, 7-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity);
    }


    /// <summary>
    /// Provides a single output, 8-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections);
    }


    /// <summary>
    /// Provides a single output, 9-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier);
    }


    /// <summary>
    /// Provides a single output, 10-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier, Object 
                            DvBottom)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom);
    }


    /// <summary>
    /// Provides a single output, 11-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier, Object 
                            DvBottom, Object DvTop)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop);
    }


    /// <summary>
    /// Provides a single output, 12-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier, Object 
                            DvBottom, Object DvTop, Object ColAbove)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove);
    }


    /// <summary>
    /// Provides a single output, 13-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier, Object 
                            DvBottom, Object DvTop, Object ColAbove, Object ColBelow)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow);
    }


    /// <summary>
    /// Provides a single output, 14-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <param name="ColFarAbove">Input argument #14</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier, Object 
                            DvBottom, Object DvTop, Object ColAbove, Object ColBelow, 
                            Object ColFarAbove)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove);
    }


    /// <summary>
    /// Provides a single output, 15-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <param name="ColFarAbove">Input argument #14</param>
    /// <param name="Vbot">Input argument #15</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier, Object 
                            DvBottom, Object DvTop, Object ColAbove, Object ColBelow, 
                            Object ColFarAbove, Object Vbot)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove, Vbot);
    }


    /// <summary>
    /// Provides a single output, 16-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <param name="ColFarAbove">Input argument #14</param>
    /// <param name="Vbot">Input argument #15</param>
    /// <param name="Vtop">Input argument #16</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier, Object 
                            DvBottom, Object DvTop, Object ColAbove, Object ColBelow, 
                            Object ColFarAbove, Object Vbot, Object Vtop)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove, Vbot, Vtop);
    }


    /// <summary>
    /// Provides a single output, 17-input Objectinterface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <param name="ColFarAbove">Input argument #14</param>
    /// <param name="Vbot">Input argument #15</param>
    /// <param name="Vtop">Input argument #16</param>
    /// <param name="Raytrace">Input argument #17</param>
    /// <returns>An Object containing the first output argument.</returns>
    ///
    public Object predictfunction(Object Area, Object Normalsimilarity, Object NormalZ, 
                            Object DiagonalXY, Object Height, Object Coplanarity, Object 
                            Proximity, Object Connections, Object Wallinlier, Object 
                            DvBottom, Object DvTop, Object ColAbove, Object ColBelow, 
                            Object ColFarAbove, Object Vbot, Object Vtop, Object Raytrace)
    {
      return mcr.EvaluateFunction("predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove, Vbot, Vtop, Raytrace);
    }


    /// <summary>
    /// Provides the standard 0-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", new Object[]{});
    }


    /// <summary>
    /// Provides the standard 1-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area);
    }


    /// <summary>
    /// Provides the standard 2-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity);
    }


    /// <summary>
    /// Provides the standard 3-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ);
    }


    /// <summary>
    /// Provides the standard 4-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY);
    }


    /// <summary>
    /// Provides the standard 5-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height);
    }


    /// <summary>
    /// Provides the standard 6-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity);
    }


    /// <summary>
    /// Provides the standard 7-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity);
    }


    /// <summary>
    /// Provides the standard 8-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections);
    }


    /// <summary>
    /// Provides the standard 9-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier);
    }


    /// <summary>
    /// Provides the standard 10-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier, Object DvBottom)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom);
    }


    /// <summary>
    /// Provides the standard 11-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier, Object DvBottom, Object DvTop)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop);
    }


    /// <summary>
    /// Provides the standard 12-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier, Object DvBottom, Object DvTop, Object ColAbove)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove);
    }


    /// <summary>
    /// Provides the standard 13-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier, Object DvBottom, Object DvTop, Object ColAbove, 
                              Object ColBelow)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow);
    }


    /// <summary>
    /// Provides the standard 14-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <param name="ColFarAbove">Input argument #14</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier, Object DvBottom, Object DvTop, Object ColAbove, 
                              Object ColBelow, Object ColFarAbove)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove);
    }


    /// <summary>
    /// Provides the standard 15-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <param name="ColFarAbove">Input argument #14</param>
    /// <param name="Vbot">Input argument #15</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier, Object DvBottom, Object DvTop, Object ColAbove, 
                              Object ColBelow, Object ColFarAbove, Object Vbot)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove, Vbot);
    }


    /// <summary>
    /// Provides the standard 16-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <param name="ColFarAbove">Input argument #14</param>
    /// <param name="Vbot">Input argument #15</param>
    /// <param name="Vtop">Input argument #16</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier, Object DvBottom, Object DvTop, Object ColAbove, 
                              Object ColBelow, Object ColFarAbove, Object Vbot, Object 
                              Vtop)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove, Vbot, Vtop);
    }


    /// <summary>
    /// Provides the standard 17-input Object interface to the predictfunction MATLAB
    /// function.
    /// </summary>
    /// <remarks>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return.</param>
    /// <param name="Area">Input argument #1</param>
    /// <param name="Normalsimilarity">Input argument #2</param>
    /// <param name="NormalZ">Input argument #3</param>
    /// <param name="DiagonalXY">Input argument #4</param>
    /// <param name="Height">Input argument #5</param>
    /// <param name="Coplanarity">Input argument #6</param>
    /// <param name="Proximity">Input argument #7</param>
    /// <param name="Connections">Input argument #8</param>
    /// <param name="Wallinlier">Input argument #9</param>
    /// <param name="DvBottom">Input argument #10</param>
    /// <param name="DvTop">Input argument #11</param>
    /// <param name="ColAbove">Input argument #12</param>
    /// <param name="ColBelow">Input argument #13</param>
    /// <param name="ColFarAbove">Input argument #14</param>
    /// <param name="Vbot">Input argument #15</param>
    /// <param name="Vtop">Input argument #16</param>
    /// <param name="Raytrace">Input argument #17</param>
    /// <returns>An Array of length "numArgsOut" containing the output
    /// arguments.</returns>
    ///
    public Object[] predictfunction(int numArgsOut, Object Area, Object Normalsimilarity, 
                              Object NormalZ, Object DiagonalXY, Object Height, Object 
                              Coplanarity, Object Proximity, Object Connections, Object 
                              Wallinlier, Object DvBottom, Object DvTop, Object ColAbove, 
                              Object ColBelow, Object ColFarAbove, Object Vbot, Object 
                              Vtop, Object Raytrace)
    {
      return mcr.EvaluateFunction(numArgsOut, "predictfunction", Area, Normalsimilarity, NormalZ, DiagonalXY, Height, Coplanarity, Proximity, Connections, Wallinlier, DvBottom, DvTop, ColAbove, ColBelow, ColFarAbove, Vbot, Vtop, Raytrace);
    }


    /// <summary>
    /// Provides an interface for the predictfunction function in which the input and
    /// output
    /// arguments are specified as an array of Objects.
    /// </summary>
    /// <remarks>
    /// This method will allocate and return by reference the output argument
    /// array.<newpara></newpara>
    /// M-Documentation:
    /// #function TreeBagger classificationEnsemble ClassificationECOC
    /// ClassificationBaggedEnsemble predict
    /// </remarks>
    /// <param name="numArgsOut">The number of output arguments to return</param>
    /// <param name= "argsOut">Array of Object output arguments</param>
    /// <param name= "argsIn">Array of Object input arguments</param>
    /// <param name= "varArgsIn">Array of Object representing variable input
    /// arguments</param>
    ///
    [MATLABSignature("predictfunction", 17, 1, 0)]
    protected void predictfunction(int numArgsOut, ref Object[] argsOut, Object[] argsIn, params Object[] varArgsIn)
    {
        mcr.EvaluateFunctionForTypeSafeCall("predictfunction", numArgsOut, ref argsOut, argsIn, varArgsIn);
    }

    /// <summary>
    /// This method will cause a MATLAB figure window to behave as a modal dialog box.
    /// The method will not return until all the figure windows associated with this
    /// component have been closed.
    /// </summary>
    /// <remarks>
    /// An application should only call this method when required to keep the
    /// MATLAB figure window from disappearing.  Other techniques, such as calling
    /// Console.ReadLine() from the application should be considered where
    /// possible.</remarks>
    ///
    public void WaitForFiguresToDie()
    {
      mcr.WaitForFiguresToDie();
    }



    #endregion Methods

    #region Class Members

    private static MWMCR mcr= null;

    private static Exception ex_= null;

    private bool disposed= false;

    #endregion Class Members
  }
}
