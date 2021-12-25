﻿using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class AutomaticWeaponType : Weapon
{
	public override async Task Reload(ReloadAction reload)
	{
		Debug.Log($"start reloading");
		//yield return new WaitForSeconds(2f);
		weaponType.bulletLeft = weaponType.maxMagazine;
		weaponType.reloading = false;
		Debug.Log($"finish reloading");
		player.FinishAction(reload);
		await Task.Yield();
	}

	public void Shoot(RaycastHit hit)
	{
		// slow don the rate of shooting using delay method
		if (weaponType.readyToShoot)
		{
			weaponType.readyToShoot = false;
			StartCoroutine(DelayShooting());
		}
		// hi is always a point in the center of the fps_screen ( always have fixed height
		// and width) we get the node exactly under that point
		//Node hitNode = grid.getNodeFromTransformPosition(null, hit.point);
		//Vector3 targetPoint = hitNode.coord;
		// this is the direction between the player node to the hit point node
		Vector3 dir = player.currentTarget.aimPoint.position - startPoint.position;

		// sp to different direction around the target
		float x = Random.Range(-weaponType.spread, weaponType.spread);
		float y = Random.Range(-weaponType.spread, weaponType.spread);

		dir = dir + new Vector3(x, y, 0);
		//Debug.Log($"hit point {hit.point}  node.coord {hitNode.coord} dir {dir}");

		Ammo bullet = Instantiate(weaponType.Ammo, startPoint.position, Quaternion.identity);
		// we orient the bullet to the direction created
		bullet.transform.forward = dir.normalized;

		Rigidbody rb = bullet.GetComponent<Rigidbody>();
		// the bullet create at any point folow the direction (wich have 0 on y at any
		// direction) so the bullet created at the startPoint stays on the same height
		//rb.AddForce(fps_Cam.transform.up * weaponType.bouncingForce, ForceMode.Impulse);
		if (weaponType.bouncingForce > 0)
			rb.AddForce(startPoint.forward * weaponType.bouncingForce * Time.fixedDeltaTime, ForceMode.Impulse);
		else
			rb.AddForce(dir * weaponType.ammoSpeed * Time.fixedDeltaTime, ForceMode.Impulse);

		weaponType.bulletLeft--;
		weaponType.bulletsShot++;
	}

	public override async Task startShooting(ShootAction shoot)
	{
		// need to read documentation on the ViewportPointToRay method Vector3(0.5f, 0.5f,
		// 0) the ray is at the center of the camera view. The bottom-left of the camera is
		// (0,0); the top-right is (1,1).

		if (weaponType.readyToShoot && !weaponType.reloading && weaponType.bulletLeft > 0)
		{
			Vector3 dir = (player.currentTarget.aimPoint.position - startPoint.position).normalized;
			ParticleSystem effectObj = Instantiate(weaponType.Ammo.fireEffect, startPoint.position, player.partToRotate.rotation);
			ParticleSystem effect = effectObj.GetComponent<ParticleSystem>();

			var main = effect.main;
			main.duration = weaponType.timeBetweenShooting;

			RaycastHit hit;
			if (Physics.Raycast(startPoint.position, dir, out hit, weaponType.bulletRange))
			{
				weaponType.bulletsShot = 0;
				effect.Play(true);

				while (weaponType.bulletsShot <= weaponType.bulletInOneShot)
				{
					Shoot(hit);
					await Task.Delay((int)(weaponType.timeBetweenShooting * 1000));
				}
				effect.Stop(true);
				Destroy(effect.gameObject);
			}
			else
			{
				Debug.Log($" out of range!  bullet range is  {weaponType.bulletRange} ");
			}
		}
		Debug.Log($"finish shotting");
		player.FinishAction(shoot);
	}

	private IEnumerator DelayShooting()
	{
		yield return new WaitForSeconds(weaponType.timeBetweenShooting);
		weaponType.readyToShoot = true;
	}

	public override string ToString()
	{
		return $"weapon: {this.name}";
	}
}