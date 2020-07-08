import numpy as np
import pandas as pd
from random import randrange, shuffle
import math

# experient cases (input here!)
user_id = randrange(0,100000)
task_id = 0
block_id = 0

panto_bounds_x = [-12, 12]
panto_bounds_y = [-3, -16]


def is_in_bounds(point):
    if point[0] < panto_bounds_x[0] or point[0] > panto_bounds_x[1]:
        return False
    if not panto_bounds_y[0] > point[1] > panto_bounds_y[1]:
        return False
    return True


def get_point_in_direction(p, angle, length):
    """

    Parameters
    ----------
    p: point from where to go
    angle
    length: distance between the points

    Returns: new point in angle direction
    -------

    """

    angle = angle * math.pi /180 # angle in radians
    return [length * math.cos(angle) + p[0], length * math.sin(angle) + p[1]]


def generate_start_positions(target_positions, angles_count, distances):
    start_target_pairs = []
    for t in target_positions:
        for a in range(angles_count):
            for d in distances:
                found_start = False
                # test so many start positions until we found one that is in panto bounds
                while not found_start:
                    angle = randrange(0, 360)
                    # add vector with angle and distance to t
                    possible_start = get_point_in_direction(t, angle, d)
                    if is_in_bounds(possible_start):
                        start_target_pairs += [(possible_start[0], possible_start[1], t[0], t[1], str(angle), str(d))]
                        found_start = True
    return start_target_pairs


target_positions   = [[-8, -7], [8,-7], [-12,-11], [12,-11], [-8,-15], [8,-15]] # 6 target positions (circular around the middle)
target_distances = [2,4,8]
trials_per_block   = 6
#conditions         = ['rail', 'control'] #rail / control condition
rail_lengths = [2,4,8]

positions = generate_start_positions(target_positions=target_positions, angles_count=3, distances=target_distances)
shuffle(positions)

df = pd.DataFrame();

# append chunks first to the task_chunks and shuffle later
task_blocks = []
for l in rail_lengths:
    # various rail lengths are the conditions
    block = []
    for p in range(len(positions)):
        pos_tuple = positions[p]
        block.append((pos_tuple + (str(l),)))
        if (p+1) % trials_per_block == 0:
            task_blocks.append(block)
            block = []

shuffle(task_blocks)

trial_id = 0
for b in range(len(task_blocks)):
    block = task_blocks[b]
    for trial in block:
        df = df.append(pd.Series((str(user_id), str(b), str(trial_id)) + trial), ignore_index=True)
        trial_id += 1

df.columns=['user_id','block_id','trial_id','starting_x','starting_y','target_x','target_y','angle','distance','rail_length']
df = df.round(2)
df.to_csv('./protocol_{}.csv'.format(user_id), index=False)
