#define mUMario
if(object_get_parent(object_index) == parPlayer)
	if(global.spin && global.spinTimer mod 5 == 0)
		d3d_instance_create(x+lengthdir_x(5,direction+180),y+lengthdir_y(5,direction+180),z,objSmoke);

if(animated_sprite_check_run())
{
        if(image_index > 4 && image_index < 6 && !stepSound)
        {
            	sound_play(sndFootsteps);
            	stepSound = true;
		d3d_instance_create(x+lengthdir_x(5,direction+180),y+lengthdir_y(5,direction+180),z,objSmoke);
        }
        else if(image_index > 0 && image_index < 1)
            	stepSound = false;
        
        if(image_index >= 0 && image_index < 2)
            	flyZ += -flyZ/1.5;
        else if((image_index >= 2 && image_index < 3) || (image_index >= 5 && image_index < 6))
            	flyZ += (1 - flyZ)/1.5; //1
        else if(image_index >= 3 && image_index < 5)
            	flyZ += (2 - flyZ)/1.5; //2
}
else
	flyZ += -flyZ/1.5;