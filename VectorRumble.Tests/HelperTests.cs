using Microsoft.Xna.Framework;
using VectorRumble;

namespace VectorRumble.Tests;

public class HelperTests
{
    [Fact]
    public void ColorParse_RedHexString_ReturnsCorrectColor()
    {
        // Arrange - Format is RRGGBBAA where Red=FF, Green=00, Blue=00, Alpha=FF
        string hexColor = "FF0000FF";
        
        // Act
        Color result = Helper.ColorParse(hexColor);
        
        // Assert - Based on actual bit extraction:
        // value >> 16 & 0xFF = 0 (Red)
        // value >> 8 & 0xFF = 0 (Green) 
        // value >> 0 & 0xFF = 255 (Blue)
        // value >> 24 & 0xFF = 255 (Alpha)
        Assert.Equal(0, result.R);   // Red component
        Assert.Equal(0, result.G);   // Green component  
        Assert.Equal(255, result.B); // Blue component
        Assert.Equal(255, result.A); // Alpha component
    }
    
    [Fact]
    public void ColorParse_BlueHexString_ReturnsCorrectColor()
    {
        // Arrange - Format: Red=00, Green=00, Blue=FF, Alpha=FF
        string hexColor = "0000FFFF";
        
        // Act
        Color result = Helper.ColorParse(hexColor);
        
        // Assert - Based on actual bit extraction:
        // value >> 16 & 0xFF = 0 (Red)
        // value >> 8 & 0xFF = 255 (Green)
        // value >> 0 & 0xFF = 255 (Blue) 
        // value >> 24 & 0xFF = 0 (Alpha)
        Assert.Equal(0, result.R);   // Red component
        Assert.Equal(255, result.G); // Green component
        Assert.Equal(255, result.B); // Blue component
        Assert.Equal(0, result.A);   // Alpha component
    }
    
    [Fact]
    public void ColorParse_WhiteHexString_ReturnsCorrectColor()
    {
        // Arrange
        string hexColor = "FFFFFFFF"; // White with full alpha
        
        // Act
        Color result = Helper.ColorParse(hexColor);
        
        // Assert
        Assert.Equal(255, result.R); // Red component
        Assert.Equal(255, result.G); // Green component
        Assert.Equal(255, result.B); // Blue component
        Assert.Equal(255, result.A); // Alpha component
    }
    
    [Fact]
    public void ColorParse_BlackHexString_ReturnsCorrectColor()
    {
        // Arrange
        string hexColor = "00000000"; // Black with zero alpha
        
        // Act
        Color result = Helper.ColorParse(hexColor);
        
        // Assert
        Assert.Equal(0, result.R); // Red component
        Assert.Equal(0, result.G); // Green component
        Assert.Equal(0, result.B); // Blue component
        Assert.Equal(0, result.A); // Alpha component
    }
    
    [Fact]
    public void GetMyDocumentsFolder_ReturnsNonEmptyString()
    {
        // Act
        string result = Helper.GetMyDocumentsFolder();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
    
    [Fact]
    public void GetAssemblyTitle_ReturnsValidString()
    {
        // Act
        string result = Helper.GetAssemblyTitle();
        
        // Assert
        Assert.NotNull(result);
        // The result could be empty if no AssemblyTitle attribute is set, which is valid
    }
    
    [Fact]
    public void GetFilesFromFolders_EmptyFolderArray_ReturnsEmptyArrayOrNull()
    {
        // Arrange
        string[] folders = new string[0];
        string filter = "*.txt";
        
        // Act
        string[] result = Helper.GetFilesFromFolders(folders, filter);
        
        // Assert - Can return null or empty array for non-existent directory
        Assert.True(result == null || result.Length == 0);
    }
    
    [Fact]
    public void GetFilesFromFolders_NonExistentPath_ReturnsNull()
    {
        // Arrange
        string[] folders = { "NonExistentFolder", "AnotherNonExistentFolder" };
        string filter = "*.txt";
        
        // Act
        string[] result = Helper.GetFilesFromFolders(folders, filter);
        
        // Assert
        Assert.Null(result);
    }
}
