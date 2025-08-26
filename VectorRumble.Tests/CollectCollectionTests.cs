using Microsoft.Xna.Framework;
using VectorRumble;

namespace VectorRumble.Tests;

public class CollectCollectionTests
{
    [Fact]
    public void Constructor_ValidWorld_SetsWorldAndInitializesGarbage()
    {
        // Arrange
        var world = new World(new Vector2(800, 600));
        
        // Act
        var collection = new CollectCollection<string>(world);
        
        // Assert
        Assert.Equal(world, collection.World);
        Assert.NotNull(collection.Garbage);
        Assert.Equal(0, collection.Garbage.Count);
    }
    
    [Fact]
    public void Add_Item_IncreasesCount()
    {
        // Arrange
        var world = new World(new Vector2(800, 600));
        var collection = new CollectCollection<string>(world);
        
        // Act
        collection.Add("test item");
        
        // Assert
        Assert.Equal(1, collection.Count);
        Assert.Contains("test item", collection);
    }
    
    [Fact]
    public void Collect_ItemsInGarbage_RemovesFromCollection()
    {
        // Arrange
        var world = new World(new Vector2(800, 600));
        var collection = new CollectCollection<string>(world);
        
        // Add items to collection
        collection.Add("item1");
        collection.Add("item2"); 
        collection.Add("item3");
        
        // Add some items to garbage
        collection.Garbage.Add("item1");
        collection.Garbage.Add("item3");
        
        // Act
        collection.Collect();
        
        // Assert
        Assert.Equal(1, collection.Count); // Only item2 should remain
        Assert.Contains("item2", collection);
        Assert.DoesNotContain("item1", collection);
        Assert.DoesNotContain("item3", collection);
        Assert.Equal(0, collection.Garbage.Count); // Garbage should be empty after collect
    }
    
    [Fact]
    public void Collect_NoGarbage_NoChange()
    {
        // Arrange
        var world = new World(new Vector2(800, 600));
        var collection = new CollectCollection<string>(world);
        
        collection.Add("item1");
        collection.Add("item2");
        
        // Act
        collection.Collect(); // No items in garbage
        
        // Assert
        Assert.Equal(2, collection.Count);
        Assert.Contains("item1", collection);
        Assert.Contains("item2", collection);
    }
    
    [Fact]
    public void Collect_EmptyCollection_NoError()
    {
        // Arrange
        var world = new World(new Vector2(800, 600));
        var collection = new CollectCollection<string>(world);
        
        // Act & Assert - Should not throw
        collection.Collect();
        
        Assert.Equal(0, collection.Count);
        Assert.Equal(0, collection.Garbage.Count);
    }
    
    [Fact]
    public void Garbage_SetProperty_UpdatesGarbageCollection()
    {
        // Arrange
        var world = new World(new Vector2(800, 600));
        var collection = new CollectCollection<string>(world);
        var newGarbage = new System.Collections.ObjectModel.Collection<string>();
        newGarbage.Add("garbage item");
        
        // Act
        collection.Garbage = newGarbage;
        
        // Assert
        Assert.Equal(newGarbage, collection.Garbage);
        Assert.Equal(1, collection.Garbage.Count);
        Assert.Contains("garbage item", collection.Garbage);
    }
}