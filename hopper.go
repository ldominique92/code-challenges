package main
import 
 (
     "fmt"
     "sort"
 )
/*

Given maxValue, int >= 0

Valid number: 0 <= x <= maxValue, x: int
blockRange: (x1, x2), both are valid number. x is blocked when x1 <= x <= x2. (3, 5) blocks 4, 3, 5

Function
Input:
  maxValue: Int
  blockRanges: list<blockRange>
  
Out:    
  Minimum valid number which is not blocked by any blockRanges


Input: maxValue = 8, blockedRanges = [(0, 3), (1,2), (3, 9), (8, 8)]
Output: 10

*/

func minValue(maxValue int,  blockRanges [][]int) int{
    minimum := 0 // 10
    
    sort.Slice(blockRanges, func(i, j int) bool {
        return blockRanges[i][0] < blockRanges[j][0]
    })
    
    for _, tuple := range blockRanges {
    
        if tuple[0] <= minimum && tuple[1] >= minimum {
            minimum = tuple[1] + 1
        }
        
        if minimum > maxValue {
            return -1
        }
    }
    
    return minimum
}

func main() {
    blockedRanges := [][]int{{0,0}, {1,2}, {3, 9}, {8, 8}}
    maxValue := 15
    res := minValue(maxValue, blockedRanges)
    fmt.Println(res)
}
