import { configureStore, createSlice } from "@reduxjs/toolkit"

// Minimal slice to fix the empty reducer issue
const appSlice = createSlice({
  name: 'app',
  initialState: { initialized: true },
  reducers: {}
});

export const store = configureStore({
  reducer:{
    app: appSlice.reducer,
  },
  //middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(logger),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
