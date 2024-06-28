#define mUGoombario
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
}

flyZ = 0;