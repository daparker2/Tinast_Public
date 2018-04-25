// 3D printable gauge pod for the 4" display setup. We'll break it into 2 major pieces:
// The top and bottom. The top is the "hat" and "roof", the bottom houses the Pi.

// Then the bottom is subdivided into left & right sides and a front and back. There is no
// bottom on the bottom since the Pi goes into the housing thru there.

// FIXMEFIXME: When done, digest this file and try to understand how to more methodically design things for printing in this language. This listing is probably unreadable at this point.

thickness=2.3;
overall_width=104;
// Height doesn't include the size of the top...
overall_height=77;
overall_length=39;
overhang_height=20;
cutout_top = 13;
cutout_left = 10;
cutout_width = 33;
cutout_height=14;
hat_length=30;
hole_pitch = 20;
hole_base = 20;
base_width = 70;
base_height=7;
base_length = 35;
base_angle = 6;
hole_radius = 2.5;
post_radius = 3;
post_height = overall_length + hat_length + thickness;
detail=100;
num_slices=6;

module rounded_corner(x, y, z, r)
{
	difference() {
		cube([x, y, z]);
		translate([r / 2 - .05, r / 2 - .05, 0]) {
			minkowski() {
				cube([x - r + .1, y - r + .1, z]);
				sphere(r=r / 2, $fn=detail);
			}
		}
	}
}

module half_rounded_corner(x, y, z, r)
{
	difference() {
		rounded_corner(x, y, z, r);
		translate([-x / 2, -y / 2, -z / 2])
			cube([2 * x, y, 2 * z]);
	}
}

module post(radius, height, bottom, top, left, right) {	
	union() {
		difference() {
			cylinder(r=radius, h=height, $fn=detail);
			if (bottom) {
				translate([right ? radius / 2 : -radius / 2,0,height - 1.9])
					cylinder(r2=radius/1.75, r1=radius/1.75, h=2, $fn=detail);
			}
		}
		if (top) {
			translate([right ? radius / 2 : -radius / 2,0,-2.1])
				cylinder(r2=radius/2, r1=radius/2-.5, h=2, $fn=detail);
		}
	}
}

module top(width,height) {
	vent_size=1.75;
	vent_x = 5;
	vent_y = 5;
	
	union() {
		difference() {
			union() { 
				translate([thickness, -thickness, 0]) 
					cube([width - 2 * thickness, height + hat_length + thickness, 2 * thickness]);
				translate([thickness, height + hat_length, 0]) 
					rotate([90, 0, 0]) 
					cylinder(r=2 * thickness, h=height + hat_length + thickness, $fn=detail);
				translate([width - thickness, height + hat_length, 0]) 
					rotate([90, 0, 0]) 
					cylinder(r=2 * thickness, h=height + hat_length + thickness, $fn=detail);
			}		
			translate([-thickness, -thickness, -(2 * thickness)]) 
				cube([width + 2 * thickness, height + hat_length + 2 * thickness, 2 * thickness]);
			
			translate([-thickness, -2.8, -66.6])
				rotate([90, 0, 90])
					half_rounded_corner(height + hat_length + thickness + 1, height + hat_length + thickness, width + 2 * thickness, 4 * thickness);
		}
	}
}

module overhang(width, height) {
	overhang_length=overall_height - height;
	union() {
		translate([-.1,-width + .1]) 
			cube([overhang_length,width,thickness]);
			translate([overhang_length,0]) 
				rotate([180, 0, 0]) 
					translate([-.1,-.1,-thickness])
						difference() {
							scale([min(width, height) / max(width, height), 1, 1])
								linear_extrude(height=thickness) 
									circle(r=max(width, height), $fn = detail);
							
							translate([ - overhang_length, -width, -.1])
								cube([overhang_length,width,thickness + .2]);
						}
	}	
}

module right(width, height) {
	
	union() {
		difference() {
			cube([height + .1, width + thickness, thickness]);
			translate([cutout_top + thickness, cutout_left + thickness, -(2 * thickness)]) 
				minkowski() {
					cube([cutout_width - thickness, cutout_height - thickness, 4 * thickness]);
					sphere(r=thickness, $fn=detail);
				}
		}
		overhang(hat_length, overhang_height);
	}
}

module left(width, height) {
	union() {
		cube([height + .1, width + thickness, thickness]);
		overhang(hat_length, overhang_height);
	}
}

module back(width,height) {
	hole_pitch = 20;
	hole_base = 20;
	hole_x = (width - hole_pitch) / 2;
	hole_y = height - hole_base - hole_radius;
	difference() {
		cube([height + .2, width, 2 * thickness]);
		translate([hole_y,hole_x,-(thickness)]) 
			cylinder(r=hole_radius,h=4 * thickness, $fn=detail);
		translate([hole_y,hole_x + hole_pitch - hole_radius,-thickness]) 
			cylinder(r=hole_radius,h=4 * thickness, $fn=detail);
	}
}

module front(width,height) {
	display_x = 3 + thickness;
	display_y = 10 + thickness;
	display_width = 90 - thickness;
	display_height = 52 - thickness;
	hole_width = 92 + thickness;
	hole_height = 53.5 + thickness;
	
	hole_radius = 2;
	hole_offset = 5;
	
	difference() {
		cube([width,height,thickness]);
		translate([display_x + thickness,display_y + thickness,-(2 / thickness)])
			minkowski() {
				cube([display_width - thickness,display_height - thickness,2 * thickness]);
				sphere(r=thickness, $fn=detail);
			}
		
		// Add mounting holes...
		translate([display_x + hole_radius,display_y - hole_offset - 1,-(2 / thickness)])
			cylinder(r=hole_radius, h=2 * thickness, $fn=detail);
		translate([display_x + hole_width - hole_radius,display_y - hole_offset - 1,-(2 / thickness)])
			cylinder(r=hole_radius, h=2 * thickness, $fn=detail);
		translate([display_x + hole_radius,display_y + hole_height + hole_offset - 1,-(2 / thickness)])
			cylinder(r=hole_radius, h=2 * thickness, $fn=detail);
		translate([display_x + hole_width - hole_radius,display_y + hole_height + hole_offset - 1,-(2 / thickness)])
			cylinder(r=hole_radius, h=2 * thickness);
	}
}

module base(hole_pitch, hole_base, base_width, base_height, base_length, base_angle)
{
	hole_radius = 2.5;
	bh = base_height * sin(base_angle);
	union() {
		difference() {
			cube([base_width, base_length, base_height]);
			translate([-.05, -.05, -.05])
				rounded_corner(base_width + .1, base_length + .1, base_height + .1, 8 * thickness);
			translate([-.05, -.05, -.05])
				rotate([90, 0, 90])
					half_rounded_corner(base_length + .1, base_height + .1, base_width + .1, base_height);
			translate([-.05, base_length -.05, -.05])
				rotate([90, 0, 0])
					half_rounded_corner(base_width + .1, base_height + .1, base_length + .1, base_height);	
				rotate([base_angle, 0, 0])
					translate([-1,-1,-20])
						cube([base_width + 2, base_length + 2, 20]);	
		}
		
		translate([thickness, base_length / 2, base_height - .1])
			rotate([90,0,0])
				difference() {
					cube([base_width - 2 * thickness, hole_base + 2 * hole_radius  + 3 * thickness, thickness]);
					translate([-0.05,hole_base + 2 * hole_radius + 3 * thickness + 0.05, -0.05])
						rotate([90,0,0])
							rounded_corner(base_width - 2 * thickness + .1, thickness + .1, hole_base + 2 * hole_radius + 3 * thickness + .1, thickness); 
					
					// Add speed holes...
					#translate([(base_width - 2 * thickness - hole_pitch) / 2 + hole_radius,hole_base + hole_radius + base_height,-base_length / 2]) {
						
		translate([0,-hole_radius - thickness,-overall_length / 2 + thickness]) 
			cylinder(r=hole_radius,h=overall_length, $fn=detail);
		translate([0 + hole_pitch - hole_radius,-hole_radius - thickness,-overall_length / 2+ thickness]) 
			cylinder(r=hole_radius,h=overall_length, $fn=detail);
					}
					
					translate([-thickness,-thickness - .7,thickness + .05])
						rotate([0,90,0])
							half_rounded_corner(thickness + .1, base_length, base_width, thickness);
			}
	}
}

module podule_buttresses(buttress_height, bottom, top) {
	rotate([90,0,0]) translate([0,0,-overall_length - hat_length]) {
		translate([-thickness,0,0])
			post(post_radius, buttress_height, bottom, top, true, false);
		translate([overall_width + thickness,0,0])
			post(post_radius, buttress_height, bottom, top, false, true);
		translate([overall_width + thickness,-50,0])
			post(post_radius, buttress_height, bottom, top, false, true);
		translate([-thickness,-cutout_top + post_radius,0])
			post(post_radius, buttress_height, bottom, top, true, false);
		translate([-thickness,-cutout_top - cutout_width - post_radius,0])
			post(post_radius, buttress_height, bottom, top, true, false);
	}
}

module podule_body() {
	difference() {
		union() {
			union() {
				/* add panels */
				top(overall_width, overall_length);
				translate([0, overall_length, 0]) 
					rotate([180, 90, 0]) 
					right(overall_length, overall_height);
				translate([overall_width + thickness, overall_length, 0]) 
					rotate([180, 90, 0])
					left(overall_length, overall_height);
				translate([-.1,thickness,.1])
					rotate([0,90,-90])
					back(overall_width + .2, overall_height);
				translate([overall_width,overall_length,0])
					rotate([-90,0,180])
					front(overall_width,overall_height);
				
				/* Add rounded corners for strengthening */
				translate([0,thickness,-overall_height])
					rounded_corner(overall_width, overall_length - 2 * thickness, overall_height, 2 * thickness);
				translate([overall_width,2 * overall_length - thickness - thickness,-overall_height])
					rotate([0, 0, 180])
						half_rounded_corner(overall_width, overall_length - 2 * thickness, overall_height, 2 * thickness);
				translate([0,overall_length + hat_length,-overall_width])
					rotate([90, 0, 0])
						half_rounded_corner(overall_width, overall_width, overall_length + hat_length, 2 * thickness);
				translate([0,thickness,-overall_width])
					rotate([90, 0, 90])
						half_rounded_corner(overall_length - 2 * thickness, overall_width, overall_width, 2 * thickness);
				translate([0, overall_length,-overall_width])
					rotate([90, 0, 90])
						difference() {
							half_rounded_corner(overall_length - 2 * thickness, overall_width, overall_width, 2 * thickness);
							translate([overall_length / 2, -.05, -.05])
								cube([overall_length / 2, overall_width + .1, overall_width + .1]);
						}
					}
				}
	}
}

module podule(base, pod, slice) {
	if (pod) {
		sl = (overall_length + hat_length + thickness) / num_slices;
		rotate([90,0,0])
			translate([-(overall_width / 2),-(overall_length / 2), overall_height / 2])
				union() {
					if (slice) {
						cx = overall_width + 4 * thickness + 1;
						cy = overall_height + 4 * thickness + 1;
						
						difference() {
							podule_body();
							for (i=[1:num_slices]) {
								sz = sl * (i - 2) - thickness;
								
								if (i - 1 != slice) {
									// Slice off the bottom
									translate([-2 * thickness, sz - .01, -cy + 3 * thickness])
										cube([cx, sl + .01, cy]);
								}
								
								if (i + 1 != slice) {
									// Slice off the top
									translate([-2 * thickness, sz + 2 * sl - .01, -cy + 3 * thickness])
										cube([cx, sl + .01, cy]);
								}
							}
						}
						
					} else {
							podule_body();
					
							/* add base for overlay preview */
							if (base) {
								translate([2 * (overall_width - base_width) + hole_pitch, base_length / 2 + 2 * thickness, -overall_height - hole_radius - base_height])
									rotate([0,0,180])
										#base(hole_pitch, hole_base, base_width, base_height, base_length, base_angle);
							}
						}
								
						/* add strengthening posts for ABS */
						if (slice) {
							translate([0,-(sl * (num_slices - slice)),0])
								podule_buttresses(post_height / num_slices, slice == 1 ? false : true, slice == num_slices ? false : true);
						} else {
							podule_buttresses(post_height, false, false);
						}
		}
	}
	else if (base)
	{
		rotate([-base_angle, 0, 0])
			base(hole_pitch, hole_base, base_width, base_height, base_length, base_angle);
	}
}

// Uncomment to print pod...
//podule(false, true, 1);
//podule(false, true, 2);
//podule(false, true, 3);
//podule(false, true, 4);
//podule(false, true, 5);
//podule(false, true, 6);
// Uncomment to print base...
podule(true, false);
// Uncomment for both...
//podule(true, true);