using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.DatabaseServices;

using StageWorkScripts.Models;

namespace StageWorkScripts.Functions;
internal class ModelCreation
{
    private DataImport _dataImport;
    private Variables _variables;
    public ModelCreation(Variables variables)
    {
        _dataImport = new DataImport();
        _variables = variables;
    }

    public ConstructionPlotModel GenerateModelsFromAutocad()
    {
        //Getting plot border as polyline
        List<Polyline> plotBorders = _dataImport.GetAllElementsOfTypeOnLayer<Polyline>(_variables.plotBorderLayer);

        //Creating Models
        var curbModels = GetCurbs(plotBorders);
        var pavementModels = GetPavements(plotBorders);
        var greeneryItemModels = GetGreeneryItems(plotBorders);
        var greeneryAreaModels = GetGreeneryAreas(plotBorders);
        var streetFurnitureModels = GetStreetFurniture();

        return new ConstructionPlotModel(pavementModels, greeneryItemModels, greeneryAreaModels, streetFurnitureModels, curbModels, plotBorders.Sum(x => x.Area), 0);
    }

    internal List<StreetFurnitureModel> GetStreetFurniture()
    {
        //Getting layer lists from drawing
        List<string> streetFurnitureLayers = _dataImport.GetAllLayersContainingString(_variables.streetFurnitureLayerStart);
        //Getting layer elements for each layer
        List<List<BlockReference>> streetFurniture = new();
        foreach (var streetFurnitureLayer in streetFurnitureLayers)
        {
            streetFurniture.Add(_dataImport.GetAllElementsOfTypeOnLayer<BlockReference>(streetFurnitureLayer));
        }
        //Creating models
        List<StreetFurnitureModel> streetFurnitureModels = new();
        foreach (var furn in streetFurniture)
        {
            var attr = _dataImport.GetAllAttributesFromBlockReferences(furn);
            for (var i = 0; i < furn.Count; i++)
            {
                streetFurnitureModels.Add(new StreetFurnitureModel(attr[i], furn[i].Position));
            }
        }
        return streetFurnitureModels;
    }

    internal List<CurbModel> GetCurbs(List<Polyline> plotBorders)
    {
        //Getting layer lists from drawing
        List<string> curbLayers = _dataImport.GetAllLayersContainingString(_variables.curbLayerStart);
        //Getting layer elements for each layer
        List<List<Polyline>> curbs = new();
        foreach (var curbLayer in curbLayers)
        {
            curbs.Add(_dataImport.GetAllElementsOfTypeOnLayer<Polyline>(curbLayer));
        }
        //Checking if curbs are inside plot or not
        List<List<bool>> areCurbsInsidePlot = new();
        foreach (var curbList in curbs)
        {
            areCurbsInsidePlot.Add(FunctionsPrepairingData.AreObjectsInsidePlot(plotBorders, curbList));
        }
        //Creating new models for each element
        List<CurbModel> curbModels = new();
        for (int i = 0; i < curbLayers.Count; i++)
        {
            for (var j = 0; j < curbs[i].Count; j++)
            {
                curbModels.Add(new CurbModel(curbLayers[i], curbs[i][j].Length, areCurbsInsidePlot[i][j]));
            }
        }
        return curbModels;
    }
    internal List<IPavement> GetPavements(List<Polyline> plotBorders)
    {
        //Getting layers
        List<string> pavementLayers = _dataImport.GetAllLayersContainingString(_variables.pavementLayerStart);
        //Getting objects
        List<List<Hatch>> pavements = new();
        foreach (var pavementLayer in pavementLayers)
        {
            pavements.Add(_dataImport.GetAllElementsOfTypeOnLayer<Hatch>(pavementLayer));
        }
        //Creating models
        List<IPavement> pavementModels = new();
        for (int i = 0; i < pavementLayers.Count; i++)
        {
            var layerSplit = pavementLayers[i].Split('+');
            var arePavementsInsidePlot = FunctionsPrepairingData.AreObjectsInsidePlot(plotBorders, pavements[i]);
            var pavementAreas = FunctionsPrepairingData.GetHatchArea(pavements[i]);
            var pavementPositions = _dataImport.GetCenterOfAHatch(pavements[i]);
            switch (Array.IndexOf(_variables.typeOfPavement, layerSplit[2]))
            {
                case 0:
                    for (var j = 0; j < pavementAreas.Count; j++)
                    {
                        pavementModels.Add(new AsphaltPavementModel(layerSplit, pavementAreas[j], pavementPositions[j], arePavementsInsidePlot[j]));
                    }
                    break;
                case 1:
                    for (var j = 0; j < pavementAreas.Count; j++)
                    {
                        pavementModels.Add(new TilesPavementModel(layerSplit, pavementAreas[j], pavementPositions[j], arePavementsInsidePlot[j]));
                    }
                    break;
                case 2:
                    for (var j = 0; j < pavementAreas.Count; j++)
                    {
                        pavementModels.Add(new LooseFillPavementModel(layerSplit, pavementAreas[j], pavementPositions[j], arePavementsInsidePlot[j]));
                    }
                    break;
                case 3:
                    for (var j = 0; j < pavementAreas.Count; j++)
                    {
                        pavementModels.Add(new ConcretePavementModel(layerSplit, pavementAreas[j], pavementPositions[j], arePavementsInsidePlot[j]));
                    }
                    break;
                case 4:
                    for (var j = 0; j < pavementAreas.Count; j++)
                    {
                        pavementModels.Add(new RubberPavementModel(layerSplit, pavementAreas[j], pavementPositions[j], arePavementsInsidePlot[j]));
                    }
                    break;
                case 5:
                    for (var j = 0; j < pavementAreas.Count; j++)
                    {
                        pavementModels.Add(new GrassPavementModel(layerSplit, pavementAreas[j], pavementPositions[j], arePavementsInsidePlot[j]));
                    }
                    break;
                default:
                    throw new Exception("Неизвестный тип покрытия");
            }
        }
        return pavementModels;
    }

    internal List<IGreeneryItem> GetGreeneryItems(List<Polyline> plotBorders)
    {
        //Getting layers
        List<string> greeneryItemsLayers = _dataImport.GetAllLayersContainingString(_variables.greeneryLayerStart + "+" + _variables.ItemOrArea[0]);
        //Getting objects on layers
        List<List<BlockReference>> greeneryItems = new();
        foreach (var greeneryLayer in greeneryItemsLayers)
        {
            greeneryItems.Add(_dataImport.GetAllElementsOfTypeOnLayer<BlockReference>(greeneryLayer));
        }
        //Checking if greenery is inside plot or not
        List<List<bool>> areGreeneryItemsInsidePlot = new();
        foreach (var greeneryList in greeneryItems)
        {
            areGreeneryItemsInsidePlot.Add(FunctionsPrepairingData.AreObjectsInsidePlot(plotBorders, greeneryList));
        }
        //Creating Greenery Models
        List<IGreeneryItem> greeneryModels = new();
        for (int i = 0; i < greeneryItemsLayers.Count; i++)
        {
            var layerSplit = greeneryItemsLayers[i].Split('+');
            switch (Array.IndexOf(_variables.typeOfItemGreenery, layerSplit[3]))
            {
                case 0:
                case 1:
                    for (var j = 0; j < greeneryItems[i].Count; j++)
                    {
                        greeneryModels.Add(new TreeGreeneryModel(layerSplit, greeneryItems[i][j].Position, areGreeneryItemsInsidePlot[i][j]));
                    }
                    break;
                case 2:
                    for (var j = 0; j < greeneryItems[i].Count; j++)
                    {
                        greeneryModels.Add(new SingleShrubGreeneryModel(layerSplit, greeneryItems[i][j].Position, areGreeneryItemsInsidePlot[i][j]));
                    }
                    break;
                case 3:
                    for (var j = 0; j < greeneryItems[i].Count; j++)
                    {
                        greeneryModels.Add(new CreeperGreeneryModel(layerSplit, greeneryItems[i][j].Position, areGreeneryItemsInsidePlot[i][j]));
                    }
                    break;
                default:
                    throw new Exception("Неизвестный тип штучного озеленения");
            }
        }
        return greeneryModels;
    }
    internal List<IGreeneryArea> GetGreeneryAreas(List<Polyline> plotBorders)
    {
        //Getting layers
        List<string> greeneryAreaLayers = _dataImport.GetAllLayersContainingString(_variables.greeneryLayerStart + "+" + _variables.ItemOrArea[1]);
        //Getting objects on layers
        List<List<Hatch>> greeneryAreas = new();
        foreach (var greeneryAreaLayer in greeneryAreaLayers)
        {
            greeneryAreas.Add(_dataImport.GetAllElementsOfTypeOnLayer<Hatch>(greeneryAreaLayer));
        }
        //Checking if greenery is inside plot or not
        List<List<bool>> areGreeneryAreasInsidePlot = new();
        foreach (var greeneryList in greeneryAreas)
        {
            areGreeneryAreasInsidePlot.Add(FunctionsPrepairingData.AreObjectsInsidePlot(plotBorders, greeneryList));
        }
        //Creating Greenery Models
        List<IGreeneryArea> greeneryModels = new();
        for (int i = 0; i < greeneryAreaLayers.Count; i++)
        {
            var layerSplit = greeneryAreaLayers[i].Split('+');
            var hatchAreas = FunctionsPrepairingData.GetHatchArea(greeneryAreas[i]);
            var hatchPositions = _dataImport.GetCenterOfAHatch(greeneryAreas[i]);
            switch (Array.IndexOf(_variables.typeOfAreaGreenery, layerSplit[3]))
            {
                case 0:
                case 1:
                    for (var j = 0; j < greeneryAreas[i].Count; j++)
                    {
                        greeneryModels.Add(new AreaShrubsGreeneryModel(layerSplit, hatchPositions[j], hatchAreas[j], areGreeneryAreasInsidePlot[i][j]));
                    }
                    break;
                case 2:
                case 3:
                    for (var j = 0; j < greeneryAreas[i].Count; j++)
                    {
                        greeneryModels.Add(new FlowerbedGreeneryModel(layerSplit, hatchAreas[j], hatchPositions[j], areGreeneryAreasInsidePlot[i][j]));
                    }
                    break;
                case 4:
                    for (var j = 0; j < greeneryAreas[i].Count; j++)
                    {
                        greeneryModels.Add(new GrassGreeneryModel(layerSplit, hatchAreas[j], hatchPositions[j], areGreeneryAreasInsidePlot[i][j]));
                    }
                    break;
                default:
                    throw new Exception("Неизвестный тип площадного озеленения");
            }

        }
        return greeneryModels;
    }
}
