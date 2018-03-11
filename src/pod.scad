// 3D printable gauge pod for the 4" display setup. We'll break it into 2 major pieces:
// The top and bottom. The top is the "hat" and "roof", the bottom houses the Pi.

// Then the bottom is subdivided into left & right sides and a front and back. There is no
// bottom on the bottom since the Pi goes into the housing thru there.

thickness=2.3;
overall_width=104;
// Height doesn't include the size of the top...
overall_height=77;
overall_length=39;
overhang_height=20;
hat_length=30;

detail=100;

module top(width,height) {
	vent_size=1;
	vent_x = 5;
	vent_y = 5;
	
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
		
		// Add some vent holes
		translate([38, 15, -thickness]) {
			for (x = [0:5]) {
				translate([x * vent_x,0,0]) 
					cylinder(r=vent_size, h=4 * thickness, $fn=detail);
			}
			for (x = [0:4]) {
				translate([vent_x / 2 + x * vent_x,vent_y,0]) 
					cylinder(r=vent_size, h=4 * thickness, $fn=detail);
			}
			for (x = [0:5]) {
				translate([x * vent_x,2 * vent_y,0]) 
					cylinder(r=vent_size, h=4 * thickness, $fn=detail);
			}
		}
		// This could probably be cleaned up
		translate([-12,70,10])
			rotate([90, 180, 90]) 
			linear_extrude(height=120)
			polygon(points=[[0,0],[32,0],[0,4 * thickness]]);
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
			linear_extrude(height=thickness) 
			polygon(points=[[0,0],[height + .1,0],[0,width + .1]]);
	}
}

module right(width, height) {
	cutout_top = 13;
	cutout_left = 10;
	cutout_width = 33;
	cutout_height=14;
	
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
	hole_radius = 2.5;
	hole_x = 40;
	hole_y = height - 30 - hole_radius;
	difference() {
		cube([height + .2, width, 2 * thickness]);
		translate([hole_y,hole_x,-(thickness)]) 
			cylinder(r=hole_radius,h=4 * thickness, $fn=detail);
		translate([hole_y,width - hole_x - hole_radius,-thickness]) 
			cylinder(r=hole_radius,h=4 * thickness, $fn=detail);
		
		// Maybe clean this up later
		translate([72,82,-4]) 
			cylinder(r=2.25,h=4 * thickness, $fn=detail);
		translate([73.5,79.725,-4]) 
			cube([10, 4.5, 4 * thickness]);
	}
}

module front(width,height) {
	display_x = 3 + thickness;
	display_y = 11 + thickness;
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
		translate([display_x + hole_radius,display_y - hole_offset,-(2 / thickness)])
			cylinder(r=hole_radius, h=2 * thickness, $fn=detail);
		translate([display_x + hole_width - hole_radius,display_y - hole_offset,-(2 / thickness)])
			cylinder(r=hole_radius, h=2 * thickness, $fn=detail);
		translate([display_x + hole_radius,display_y + hole_height + hole_offset,-(2 / thickness)])
			cylinder(r=hole_radius, h=2 * thickness, $fn=detail);
		translate([display_x + hole_width - hole_radius,display_y + hole_height + hole_offset,-(2 / thickness)])
			cylinder(r=hole_radius, h=2 * thickness);
	}
}

translate([-(overall_width / 2),-(overall_length / 2), overall_height])
	union() {
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
	}
