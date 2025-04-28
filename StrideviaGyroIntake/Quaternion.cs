using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Quaternion
{

    public double x, y, z, w;

    public static Quaternion zero = new Quaternion(0, 0, 0, 0);

    public Quaternion normalized = Quaternion.zero;

    public Quaternion() { }


    public Quaternion(double x, double y, double z, double w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;

        getNormalized(this);
    }



    public Quaternion(double roll, double pitch, double yaw)
    {
        ToQuaternion(roll, pitch, yaw);

        getNormalized(this);
    }


    public static void getNormalized(Quaternion q)
    {
        double norm = q.getNormOf(q);

        q.normalized = q;

        q.normalized.x /= norm;
        q.normalized.y /= norm;
        q.normalized.z /= norm;
        q.normalized.w /= norm;
    }

    /**
     * @param roll
     * @param pitch
     * @param yaw
     * (roll, pitch ,yaw) represents the Euler angles.
     * We assume these values are in RADIANS.
     */
    public void ToQuaternion(double roll, double pitch, double yaw)
    {

        double cr = Math.Cos(roll * 0.5);
        double sr = Math.Sin(roll * 0.5);

        double cp = Math.Cos(pitch * 0.5);
        double sp = Math.Sin(pitch * 0.5);

        double cy = Math.Cos(yaw * 0.5);
        double sy = Math.Sin(yaw * 0.5);

        w = (cr * cp * cy + sr * sp * sy);
        x = (sr * cp * cy - cr * sp * sy);
        y = (cr * sp * cy + sr * cp * sy);
        z = (cr * cp * sy - sr * sp * cy);

        getNormalized(this);
    }

    private Quaternion getConjugateOf(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, q.w);
    }

    private double getNormOf(Quaternion q) { return Math.Sqrt((q.x * q.x) + (q.y * q.y) + (q.z * q.z) + (q.w * q.w)); }

    private double getSqrNormOf(Quaternion q) { double norm = getNormOf(q); return norm * norm; }

    public static Quaternion getInvOf(Quaternion q)
    {
        Quaternion qConj = q.getConjugateOf(q);

        double qNormSqr = q.getSqrNormOf(q);

        return new Quaternion(qConj.x / qNormSqr, qConj.y / qNormSqr, qConj.z / qNormSqr, qConj.w / qNormSqr);
    }

    public float toAngle()
    {
        return (float)(2 * Math.Acos(this.normalized.w));
    }

    public static Quaternion operator *(Quaternion q1, Quaternion q2)
    {
        double nw = q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z;
        double nx = q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y;
        double ny = q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x;
        double nz = q1.w * q2.z + q1.x * q2.y - q1.y * q2.x + q1.z * q2.w;

        return new Quaternion(nx, ny, nz, nw);
    }

}