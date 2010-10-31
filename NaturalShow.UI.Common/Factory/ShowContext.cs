using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Diagnostics;
using NaturalShow.UI.Common.Models;
using NaturalShow.UI.Common.Views;
using System.Windows;
using System.Windows.Media;
using Blake.NUI.WPF.Utility;
using System.Windows.Media.Imaging;
using Blake.NUI.WPF.Common;

namespace NaturalShow.UI.Common.Factory
{
    public delegate ShowView CreateViewHandler(ShowModel model);

    public class ShowContext
    {
        #region Fields

        object saveLockObj = new object();

        Dictionary<Type, CreateViewHandler> customTypeFactories = new Dictionary<Type, CreateViewHandler>();

        #endregion

        #region Properties

        #region DefaultContext

        private static ShowContext _context = null;
        public static ShowContext DefaultContext
        {
            get
            {
                if (_context == null)
                {
                    _context = new ShowContext();
                }
                return _context;
            }
        }

        #endregion

        #region Serializer

        private XmlSerializer _serializer = null;
        private XmlSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = CreateSerializer();
                }
                return _serializer;
            }
            set
            {
                _serializer = value;
            }
        }

        #endregion

        #region Models

        private ShowModelCollection _models = new ShowModelCollection();
        public ShowModelCollection Models
        {
            get
            {
                return _models;
            }
        }

        #endregion

        #region ShowSettings

        public ShowSettings ShowSettings
        {
            get
            {
                return Models.OfType<ShowSettings>().FirstOrDefault();
            }
        }

        #endregion

        #endregion

        #region Constructors

        public ShowContext()
        {
        }

        #endregion

        #region Factory Methods

        public void RegisterViewFactory(Type model, CreateViewHandler createViewHandler)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (createViewHandler == null)
                throw new ArgumentNullException("createViewHandler");

            if (!customTypeFactories.ContainsKey(model))
            {
                customTypeFactories.Add(model, createViewHandler);
                _serializer = null;
            }
        }

        public ShowView CreateView(ShowModel model)
        {
            Type modelType = model.GetType();

            if (customTypeFactories.ContainsKey(modelType))
            {
                CreateViewHandler handler = customTypeFactories[modelType];
                return handler(model);
            }

            if (model is TokenModel)
            {
                return null;
            }
            if (model is ShowSettings)
            {
                return null;
            }
            if (model is ShowZoneModel)
            {
                ShowZoneModel zoneModel = model as ShowZoneModel;
                //if (zoneModel.IsMap)
                //{
                //    return new ShowMapView(zoneModel);
                //}
                //else
                {
                    return new ShowZoneView(zoneModel);
                }
            }
            if (model is ShowTextModel)
            {
                return new ShowTextView(model as ShowTextModel);
            }
            if (model is ShowImageModel)
            {
                return new ShowImageView(model as ShowImageModel);
            }
            //if (model is ShowArrowModel)
            //{
            //    return new ShowArrowView(model as ShowArrowModel);
            //}
            if (model is ShowSlidePartModel)
            {
                return new ShowSlidePartView(model as ShowSlidePartModel);
            }
            if (model is ShowObjectModel)
            {
                //Model is just a generic ShowObject
                return new ShowObjectView(model as ShowObjectModel);
            }
            if (model is ShowModel)
            {
                return new ShowView(model);
            }

            return null;
        }

        public List<ShowView> CreateViews()
        {
            List<ShowView> views = new List<ShowView>();

            foreach (ShowModel model in Models)
            {
                ShowView view = CreateView(model);
                if (view != null)
                    views.Add(view);
            }

            return views;
        }

        #endregion

        #region Serialization Methods

        private XmlSerializer CreateSerializer()
        {
            Type[] builtInTypes = {     
                                typeof(ShowImageModel),
                                typeof(ShowModel),
                                typeof(ShowObjectModel),
                                typeof(ShowTextModel),
                                typeof(ShowZoneModel),
                                typeof(ShowSlidePartModel),
                                typeof(TokenModel),
                                typeof(ShowSettings)};

            List<Type> typeList = customTypeFactories.Keys.ToList();
            typeList.AddRange(builtInTypes.ToList());
            Type[] typeArray = typeList.ToArray();

            return new XmlSerializer(typeof(ShowModelCollection), typeArray);
        }

        public void Save(string path)
        {
            if (path == null || path.Length == 0)
                return;

            lock (saveLockObj)
            {
                using (Stream stream = File.Open(path, FileMode.OpenOrCreate))
                {
                    try
                    {
                        stream.SetLength(0);
                        Serializer.Serialize(stream, this.Models);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error saving: " + e.ToString());
                    }
                }
            }
        }

        public void Load(string path)
        {
            lock (saveLockObj)
            {
                using (Stream stream = File.Open(path, FileMode.OpenOrCreate))
                {
                    try
                    {
                        _models = Serializer.Deserialize(stream) as ShowModelCollection;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error loading: " + e.ToString());
                    }
                }

                foreach (ShowModel model in Models)
                {
                    model.DisplayMatrix.IsDirty = true;

                    ShowSlidePartModel slide = model as ShowSlidePartModel;
                    if (slide != null)
                    {
                        slide.Parent = GetModelByID(slide.ParentID) as ShowSlidePartModel;
                    }
                }

            }
        }

        public void SaveSettings(string showFilePath, BitmapSource source, DisplayMatrix displayMatrix)
        {
            ShowSettings showSettings = this.Models.FirstOrDefault(m => m.GetType() == typeof(ShowSettings)) as ShowSettings;

            this.Models.RemoveAll(m => m.GetType() == typeof(ShowSettings));

            if (showSettings == null)
            {
                showSettings = new ShowSettings();
            }

            showSettings.PrimaryDisplayMatrix = displayMatrix;
            showSettings.ShowPath = showFilePath;

            string imagePath = showFilePath + ".png";

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
            }

            showSettings.ImagePath = imagePath;
            this.Models.Insert(0, showSettings);
        }

        public ShowSettings LoadShowSettingsFromFile(string path)
        {
            ShowModelCollection models;

            try
            {
                using (Stream stream = File.Open(path, FileMode.OpenOrCreate))
                {
                    models = Serializer.Deserialize(stream) as ShowModelCollection;

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading: " + e.ToString());
                return null;
            }

            if (models == null)
                return null;

            foreach (ShowModel model in models)
            {
                ShowSettings showSettings = model as ShowSettings;
                if (showSettings != null)
                {
                    showSettings.ShowPath = path;
                    return showSettings;
                }
            }
            return null;
        }

        #endregion

        public void SetLocked(bool isLocked)
        {
            foreach (ShowModel model in Models)
            {
                model.IsLocked = isLocked;
            }
        }

        public void SendToFront(ShowModel selectedModel)
        {
            if (selectedModel == null)
            {
                return;
            }

            selectedModel.DisplayMatrix.ZIndex = 100000;

            foreach (ShowModel model in this.Models)
            {
                if (!model.Equals(selectedModel))
                {
                    model.DisplayMatrix.ZIndex--;
                }
            }
        }

        public ShowModel GetModelByID(Guid guid)
        {
            foreach (ShowModel model in this.Models)
            {
                if (model.ModelID.Equals(guid))
                {
                    return model;
                }
            }
            return null;
        }
    }
}
