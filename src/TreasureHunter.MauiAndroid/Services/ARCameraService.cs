using Microsoft.Maui.Devices.Sensors;

namespace TreasureHunter.MauiAndroid.Services;

public interface IARCameraService
{
    Task<bool> StartSensors();
    void StopSensors();
    (float x, float y) CalculateScreenPosition(
        double treasureLat, 
        double treasureLon, 
        Location currentLocation,
        float screenWidth,
        float screenHeight);
    double CurrentHeading { get; }
    double CurrentPitch { get; }
    double CurrentRoll { get; }
    event EventHandler? SensorDataChanged;
}

public class ARCameraService : IARCameraService
{
    private bool _compassStarted;
    private bool _orientationStarted;
    private double _currentHeading;
    private double _currentPitch;
    private double _currentRoll;

    public double CurrentHeading => _currentHeading;
    public double CurrentPitch => _currentPitch;
    public double CurrentRoll => _currentRoll;

    public event EventHandler? SensorDataChanged;

    public ARCameraService()
    {
        if (Compass.Default.IsSupported)
        {
            Compass.Default.ReadingChanged += OnCompassReadingChanged;
        }

        if (OrientationSensor.Default.IsSupported)
        {
            OrientationSensor.Default.ReadingChanged += OnOrientationReadingChanged;
        }
    }

    public async Task<bool> StartSensors()
    {
        try
        {
            if (Compass.Default.IsSupported && !_compassStarted)
            {
                Compass.Default.Start(SensorSpeed.UI);
                _compassStarted = true;
            }

            if (OrientationSensor.Default.IsSupported && !_orientationStarted)
            {
                OrientationSensor.Default.Start(SensorSpeed.UI);
                _orientationStarted = true;
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting sensors: {ex.Message}");
            return false;
        }
    }

    public void StopSensors()
    {
        if (_compassStarted)
        {
            Compass.Default.Stop();
            _compassStarted = false;
        }

        if (_orientationStarted)
        {
            OrientationSensor.Default.Stop();
            _orientationStarted = false;
        }
    }

    private void OnCompassReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        _currentHeading = e.Reading.HeadingMagneticNorth;
        SensorDataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnOrientationReadingChanged(object? sender, OrientationSensorChangedEventArgs e)
    {
        var data = e.Reading.Orientation;
        
        // Convert quaternion to Euler angles
        _currentPitch = Math.Atan2(
            2.0 * (data.W * data.X + data.Y * data.Z),
            1.0 - 2.0 * (data.X * data.X + data.Y * data.Y)
        ) * 180.0 / Math.PI;

        _currentRoll = Math.Asin(
            2.0 * (data.W * data.Y - data.Z * data.X)
        ) * 180.0 / Math.PI;

        SensorDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public (float x, float y) CalculateScreenPosition(
        double treasureLat,
        double treasureLon,
        Location currentLocation,
        float screenWidth,
        float screenHeight)
    {
        // Calculate bearing to treasure
        var bearing = CalculateBearing(
            currentLocation.Latitude,
            currentLocation.Longitude,
            treasureLat,
            treasureLon
        );

        // Calculate relative angle from device heading
        var relativeAngle = NormalizeAngle(bearing - _currentHeading);

        // Camera field of view (typical phone camera ~60-70 degrees)
        const double fovHorizontal = 65.0;
        const double fovVertical = 45.0;

        // Check if treasure is within camera view
        if (Math.Abs(relativeAngle) > fovHorizontal / 2)
        {
            return (-1, -1); // Not visible
        }

        // Calculate horizontal screen position
        var normalizedX = (relativeAngle / fovHorizontal) + 0.5;
        var screenX = (float)(normalizedX * screenWidth);

        // Calculate vertical position based on pitch
        // Simplified: treasures appear in center vertically, adjusted by pitch
        var verticalOffset = (float)(_currentPitch / fovVertical);
        var screenY = (screenHeight / 2) + (verticalOffset * screenHeight / 2);

        return (screenX, screenY);
    }

    private static double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = ToRadians(lon2 - lon1);
        var y = Math.Sin(dLon) * Math.Cos(ToRadians(lat2));
        var x = Math.Cos(ToRadians(lat1)) * Math.Sin(ToRadians(lat2)) -
                Math.Sin(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Cos(dLon);
        
        var bearing = Math.Atan2(y, x);
        return NormalizeAngle(ToDegrees(bearing));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;
    
    private static double NormalizeAngle(double angle)
    {
        angle = angle % 360;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }
}