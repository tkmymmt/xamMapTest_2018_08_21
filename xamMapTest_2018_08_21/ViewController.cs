using System;
using System.Linq;
using UIKit;
using MapKit;
using CoreLocation;
using Foundation;

namespace xamMapTest_2018_08_21
{
	public partial class ViewController : UIViewController
	{
		CLLocationManager _locationManager;

		MKMapView _mapView;

		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(true);
			if(!_mapView.Annotations.Any())
			{
				_mapView.RemoveAnnotations(_mapView.Annotations);
			}
			if (_mapView.Overlays != null)
			{
				_mapView.RemoveOverlays(_mapView.Overlays);
			}

			#region Test

			var loc = new MyCLLocationManagerDelegate(_locationManager, _mapView);
			loc.Test();

#endregion

			_locationManager.StartUpdatingLocation();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_locationManager = new CLLocationManager();
			_mapView = new MKMapView
			{
				
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			_locationManager.Delegate = new MyCLLocationManagerDelegate(_locationManager, _mapView);

			var status = CLLocationManager.Status;
			if (status == CLAuthorizationStatus.NotDetermined)
			{
				Console.WriteLine("didChangeAuthorizationStatus");
				_locationManager.RequestAlwaysAuthorization();
			}

			_locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
			_locationManager.DistanceFilter = 300;

			View.AddSubview(_mapView);

			_mapView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor).Active = true;
			_mapView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor).Active = true;
			_mapView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
			_mapView.HeightAnchor.ConstraintEqualTo(View.HeightAnchor).Active = true;

			_mapView.Delegate = new MyMKMapViewDelegate();

			_locationManager.DesiredAccuracy = CLLocation.AccuracyBest;

			_locationManager.DistanceFilter = 300;
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		class MyCLLocationManagerDelegate : CLLocationManagerDelegate
		{
			CLLocationManager _locationManager;
			CLLocationCoordinate2D _userLocation;
			CLLocationCoordinate2D _destination = new CLLocationCoordinate2D(35.727772, 139.770987);

			MKMapView _mapView;

			public MyCLLocationManagerDelegate(CLLocationManager locationManager, MKMapView mapView)
			{
				_locationManager = locationManager;
				_mapView = mapView;
			}

			void getRoute()
			{
				var destLocAnnotation = new MKPointAnnotation();
				destLocAnnotation.Coordinate = _destination;
				destLocAnnotation.Title = "聖地";

				_mapView.AddAnnotation(destLocAnnotation);

				var fromPlace = new MKMapItem(new MKPlacemark(_userLocation));
				var toPlace = new MKMapItem(new MKPlacemark(_destination));

				var request = new MKDirectionsRequest
				{
					Source = fromPlace,
					Destination = toPlace,
					RequestsAlternateRoutes = false,
					TransportType = MKDirectionsTransportType.Any
				};

				var directions = new MKDirections(request);
				directions.CalculateDirections((r, e) =>
				{
					if (r == null || !r.Routes.Any())
					{
						return;
					}

					var route = r.Routes[0];

					_mapView.AddOverlay(route.Polyline);
				});
			}

			void showUserAndDestinationOnMap()
			{
				var maxLat = Math.Max(_userLocation.Latitude, _destination.Latitude);
				var maxLon = Math.Max(_userLocation.Longitude, _destination.Longitude);
				var minLat = Math.Min(_userLocation.Latitude, _destination.Latitude);
				var minLon = Math.Min(_userLocation.Longitude, _destination.Longitude);

				var mapMargin = 1.5;
				var leastCoordspan = 0.005;
				var span_x = Math.Max(leastCoordspan, Math.Abs(maxLat - minLat) * mapMargin);
				var span_y = Math.Max(leastCoordspan, Math.Abs(maxLon - minLon) * mapMargin);

				var span = new MKCoordinateSpan(span_x, span_y);

				var center = new CLLocationCoordinate2D((maxLat + minLat) / 2, (maxLon + minLon) / 2);
				var region = new MKCoordinateRegion(center, span);

				_mapView.SetRegion(_mapView.RegionThatFits(region), true);
			}

			[Export("locationManager:didUpdateLocations:")]
			public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
			{
				_mapView.AddAnnotation(new MKPlacemark(_destination));
				_userLocation = new CLLocationCoordinate2D(manager.Location.Coordinate.Latitude, manager.Location.Coordinate.Longitude);
				var userLocAnnotation = new MKPointAnnotation();
				userLocAnnotation.Coordinate = _userLocation;
				userLocAnnotation.Title = "現在地";

				_mapView.AddAnnotation(userLocAnnotation);

				getRoute();
				showUserAndDestinationOnMap();
			}

			[Export("locationManager:didFailWithError:")]
			public override void Failed(CLLocationManager manager, NSError error)
			{
				Console.WriteLine("locationManager error");
			}
			#region Test
			public void Test()
			{
				_userLocation = new CLLocationCoordinate2D(35.6, 139.7);
				var userLocAnnotation = new MKPointAnnotation();
				userLocAnnotation.Coordinate = _userLocation;
				userLocAnnotation.Title = "現在地";

				_mapView.AddAnnotation(userLocAnnotation);

				getRoute();
				showUserAndDestinationOnMap();
			}
#endregion
		}

		class MyMKMapViewDelegate : MKMapViewDelegate
		{
			public override MKOverlayRenderer OverlayRenderer(MKMapView mapView, IMKOverlay overlay)
			{
				if(overlay is MKPolyline)
				{
					var polylineRenderer = new MKPolylineRenderer(overlay as MKPolyline)
					{
						StrokeColor = UIColor.Blue,
						LineWidth = 5
					};
					return polylineRenderer;
				}

				return null;
			}
		}
	}
}
